//https://exceptionnotfound.net/using-middleware-to-log-requests-and-responses-in-asp-net-core/
using System;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using static BVA200.Logic.LinuxCompatibility;
using static BVA200.Logic.Common;
using BVA200.Models;
using Dapper;
using System.Linq;
using Newtonsoft.Json;

namespace BVA200.Middleware
{
    public class AuthMiddleware
    {
        private static Int32 timeout = -1; //minutes
        private readonly RequestDelegate _next;
        private String[] pathsToIgnore = new String[] { "/Home/Login", "/Home/Logout", "/", "/lib/*", "/css/*" };
        private String[] pathsToNotResetTimeout = new String[] { "/Home/Ping" };
        private String[] adminOnlyPaths = new String[] { "/Utilies/MaintenanceMode", "/QC/QCLanding" };
        private String[] adminOnlyControllers = new String[] { "QC" };
        private String[] daxorAdminOnlyPaths = new String[] { "/TopazDetector/SetupDetector" };
        private String[] daxorAdminOnlyControllers = new String[] { };

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        public async Task Invoke(HttpContext httpContext)
        {
            //first, check the timeout. If < 0, need to get it.
            //this will also be a placeholder for "First run", so we can programatically set screen turn off timer as well
            if (timeout < 0){
                timeout = Convert.ToInt32(GetSetting("session_timeout"));
                int screenTimeOut = Convert.ToInt32(GetSetting("screen_timeout")) * 60;
                if (IsLinux()){
                    RunLinuxCommand("gsettings set org.gnome.desktop.session idle-delay \""+screenTimeOut+"\"");
                }
            }


            var request = httpContext.Request;
            var headers = request.Headers;
            var endpoint = request.Path;
            var response = httpContext.Response;
            
            //uncomment this if we need to debug the body here in the middleware.
            
            // request.EnableBuffering();
            // using (var reader = new StreamReader(request.Body, System.Text.Encoding.UTF8, false, 1024, true))
            // {
            //     var body = await reader.ReadToEndAsync();
            //     request.Body.Seek(0, SeekOrigin.Begin);
            // }
           
            

            try
            {
                Boolean ignore = false;
                foreach(string pti in pathsToIgnore){
                    if (pti == endpoint){
                        ignore = true;
                    }
                    char lastCharacter = pti[pti.Length-1];
                    if (lastCharacter == '*'){
                        //wildcard
                        string tPti = pti.Replace("*","");
                        if (endpoint.ToString().IndexOf(tPti) >= 0){
                            ignore = true;
                        }
                    }
                }
                if (ignore)
                {
                    await _next(httpContext);
                    return;
                }
                

                String UserSessionGUIDFromHeader = request.Headers["UserSessionGUID"].ToString();
                String UserSessionGUIDFromQueryString = request.Query["UserSessionGUID"].ToString();

                String UserSessionGUID = null;
                if (UserSessionGUIDFromHeader != String.Empty)
                {
                    UserSessionGUID = UserSessionGUIDFromHeader;
                }
                else if (UserSessionGUIDFromQueryString != String.Empty && UserSessionGUID != "")
                {
                    UserSessionGUID = UserSessionGUIDFromQueryString;
                }

                if (UserSessionGUID == null || UserSessionGUID == "")
                {
                    if (UserSessionGUIDFromHeader == "")
                    {
                        RedirectToLogin(response);
                        return;
                    }
                    Return401(response);
                    return;
                }

                /*
                    The user passed me a GUID. So that's good. Now we need to make sure the GUID matches
                    up to a user. If not, give them the boot.
                */
                UserSession session = GetUserIDFromSessionGUID(UserSessionGUID);
                

                //if I don't have a session, I need to squash the request
                if (session == null)
                {
                    if (UserSessionGUIDFromHeader == "")
                    {
                        RedirectToLogin(response);
                        return;
                    }
                    Return401(response);
                    return;
                }
                
                //now, check admin access
                string controller = endpoint.Value;
                controller = controller.Substring(0, controller.IndexOf('/', 1));
                controller = controller.Replace("/", "");

                Boolean adminPath = Array.IndexOf(adminOnlyControllers, controller) >= 0 || Array.IndexOf(adminOnlyPaths, endpoint.Value) >= 0;
                if (adminPath && session.IsAdmin == false)
                {
                    if (UserSessionGUIDFromHeader == "")
                    {
                        RedirectToLogin(response);
                        return;
                    }
                    Return401(response);
                    return;
                }
                
                int userID = session.UserID;

                Boolean daxorAdminPath = Array.IndexOf(daxorAdminOnlyControllers, controller) >= 0 || Array.IndexOf(daxorAdminOnlyPaths, endpoint.Value) >= 0;
                if (daxorAdminPath && userID != 1)
                {
                    if (UserSessionGUIDFromHeader == "")
                    {
                        RedirectToLogin(response);
                        return;
                    }
                    
                    Return401(response);
                    return;
                }

                //Inject the UserID into the request header so the downstream code knows who the request is from
                
                request.Headers.Add("UserID", userID.ToString());
                
                TimeSpan duration = GetNow() - session.LastActive;
                Double delta = duration.TotalMinutes;
                if (delta > timeout)
                {
                    //they've exceeded the timeout
                    LogoutUser(UserSessionGUID);
                    if (UserSessionGUIDFromHeader == "")
                    {
                        RedirectToLogin(response);
                        return;
                    }
                    Return401(response);
                    return;
                }

                ignore = Array.IndexOf(pathsToNotResetTimeout, endpoint.Value) >= 0;
                if (!ignore)
                {
                    //if we get here, session is still good, just need to refresh the LastActive field
                    ResetSessionTimer(UserSessionGUID);
                }
                await _next(httpContext);
                return;
            }
            catch (Exception)
            {
                response.Clear();
                response.StatusCode = 500;
                await response.WriteAsync("Internal Server Error");
                return;
            }
        }

        private void ResetSessionTimer(String UserSessionGUID)
        {
            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                conn.Execute("UPDATE UserSession SET LastActive = @LastActive WHERE UserSessionGUID = @UserSessionGUID", new { @LastActive = GetUTCNowAsISO(), @UserSessionGUID = UserSessionGUID });
            }
        }

        private void LogoutUser(String UserSessionGUID)
        {
            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                conn.Execute("UPDATE UserSession SET LoggedOut = @LoggedOut WHERE UserSessionGUID = @UserSessionGUID", new { @UserSessionGUID = UserSessionGUID, @LoggedOut = GetUTCNowAsISO() });
            }
        }

        private void RedirectToLogin(HttpResponse response)
        {
            response.Redirect("https://localhost:5001?error=Unauthorized");
        }

        private async void Return401(HttpResponse response)
        {
            response.Clear();
            response.StatusCode = 401;
            await response.WriteAsync("Unauthorized");
        }
        public static UserSession GetUserIDFromSessionGUID(String UserSessionGUID)
        {
            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                return conn.Query<UserSession>("SELECT * FROM UserSession WHERE UserSessionGUID = @UserSessionGUID AND LoggedOut IS NULL ORDER BY ID DESC", new { @UserSessionGuid = UserSessionGUID }).FirstOrDefault();
            }
        }
    }
}
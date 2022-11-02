using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BVA200.Models;
using static BVA200.Logic.LinuxCompatibility;
using static BVA200.Logic.Common;
using System.Data;
using Dapper;
using System.Data.SQLite;
using System;

namespace BVA200.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // NEED TO CHECK IF VALID USER SESSION GUID WAS PASSED.
        public IActionResult Index()
        {
            using (IDbConnection conn = new SQLiteConnection(GetConnectionString()))
            {
                var model = new QcResultsModel();

                // Get all recent qc results.
                var mostRecentEfficiencyTimeStamp = conn.Query<DateTime>("SELECT Timestamp FROM QCLog WHERE TestType = 'Efficiency' ORDER BY ID DESC LIMIT 1").FirstOrDefault();
                model.EfficiencyAgeAcceptable = (DateTime.Now - mostRecentEfficiencyTimeStamp).TotalDays < 30;
                var mostRecentConstancyTimeStamp = conn.Query<DateTime>("SELECT Timestamp FROM QCLog WHERE TestType = 'Constancy' ORDER BY ID DESC LIMIT 1").FirstOrDefault();
                model.ConstancyAgeAcceptable = (DateTime.Now - mostRecentConstancyTimeStamp).TotalDays < 1;
                var mostRecentLinearityTimeStamp = conn.Query<DateTime>("SELECT Timestamp FROM QCLog WHERE TestType = 'Linearity' ORDER BY ID DESC LIMIT 1").FirstOrDefault();
                model.LinearityAgeAcceptable = (DateTime.Now - mostRecentLinearityTimeStamp).TotalDays < 90;

                // Check if most recent qc mode is a failure.
                var mostRecentEfficiencyFailedTimestamp = conn.Query<DateTime>("SELECT Timestamp FROM QCLog WHERE TestType = 'Efficiency' AND Result = 0 ORDER BY ID DESC LIMIT 1").FirstOrDefault();
                model.EfficiencyStatusPassed = mostRecentEfficiencyFailedTimestamp < mostRecentEfficiencyTimeStamp ? true : false;
                var mostRecentFailedConstancyTimestamp = conn.Query<DateTime>("SELECT Timestamp FROM QCLog WHERE TestType = 'Constancy' AND Result = 0 ORDER BY ID DESC LIMIT 1").FirstOrDefault();
                model.ConstancyStatusPassed = mostRecentFailedConstancyTimestamp < mostRecentConstancyTimeStamp ? true : false;
                var mostRecentFailedLinearityTimestamp = conn.Query<DateTime>("SELECT Timestamp FROM QCLog WHERE TestType = 'Linearity' AND Result = 0 ORDER BY ID DESC LIMIT 1").FirstOrDefault();
                model.LinearityStatusPassed = mostRecentFailedLinearityTimestamp < mostRecentLinearityTimeStamp ? true : false;

                // Check for recent QC Calibration.
                var mostRecentCalibrationTimeStamp = conn.Query<DateTime>("SELECT Timestamp FROM CalibrationLog ORDER BY Timestamp DESC LIMIT 1").FirstOrDefault();
                model.RecentCalibrationPassed = (DateTime.Now - mostRecentCalibrationTimeStamp).TotalDays < 1;

                return View("Index", model);
            }            
        }

        public IActionResult Login([FromBody] User user)
        {
            var now = GetUTCNowAsISO();
            var userSessionGUID = Guid.NewGuid().ToString();

            using (IDbConnection conn = new SQLiteConnection(GetConnectionString()))
            {
                // Get the user.
                var currentUser = conn.Query<User>("Select * FROM User WHERE User.UserName = @UserName AND User.Password = @Password", new { user.UserName, user.Password }).FirstOrDefault();

                // Logout all users.
                conn.Execute(@"UPDATE UserSession SET LoggedOut = @now WHERE LoggedOut IS NULL", new { @now = now });

                if (currentUser != null)
                {
                    // Create a new user session.
                    var results = conn.Execute(@"INSERT INTO UserSession(UserID, UserSessionGUID, IsAdmin, LoggedIn, LastActive)
                                            VALUES(@UserID, @UserSessionGUID, @IsAdmin, @LoggedIn, @LastActive)"
                                            , new { @UserID = currentUser.ID, @UserSessionGuid = userSessionGUID, @IsAdmin = currentUser.IsAdmin, @LoggedIn = now, @LastActive = now });

                    int sessionTimeout = Convert.ToInt32(GetSetting("session_timeout"));
                    return StatusCode(200, new LoginModel{
                        UserSessionGUID = userSessionGUID,
                        SessionTimeout = sessionTimeout
                    });
                }
                else
                {
                    return StatusCode(401);
                }
            }

        }

        public IActionResult Logout([FromHeader] Guid UserSessionGUID)
        {
            using (IDbConnection conn = new SQLiteConnection(GetConnectionString()))
            {
                var now = GetUTCNowAsISO();

                var results = conn.Execute(@"UPDATE UserSession SET LoggedOut = @now WHERE UserSessionGUID = @UserSessionGUID"
                                            , new { now, UserSessionGUID });

                return View("Index");
            }
        }

        public IActionResult DatabaseTest()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Ping()
        {
            /*
                This is used to check connection from the UI to the API
                and also to trigger a timeout check handled by the middlware
            */
            return StatusCode(200, "OK");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

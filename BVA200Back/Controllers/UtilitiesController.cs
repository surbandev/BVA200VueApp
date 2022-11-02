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
using System.Collections.Generic;

namespace BVA200.Controllers
{
    public class UtilitiesController : Controller
    {
        private readonly ILogger<UtilitiesController> _logger;

        public UtilitiesController(ILogger<UtilitiesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult GetListOfPrinters()
        {
            string[] printers = BVA200.Logic.Common.GetAvailablePrinters();
            return StatusCode(200, printers);
        }

        [HttpPost]
        public ActionResult SpeakString([FromBody] TextToSppechModel ttsm)
        {
            TTS(ttsm.StringToSpeak);
            return StatusCode(200);
        }

        [HttpPost]
        public ActionResult<object> GetSetting(String setting)
        {
            var value = Convert.ToInt32(GetSetting("session_timeout"));
            return StatusCode(200, value);
        }

        [HttpPost]

        public ActionResult<string> MaintenanceMode([FromHeader] int UserID, [FromHeader] Guid UserSessionGUID)
        {
            try
            {
                //First thing, log that user entered maintenance mode
                using (var conn = new SQLiteConnection(GetConnectionString()))
                {
                    var result = conn.Query<User>(@"INSERT INTO MaintenanceLogs (User_ID, Timestamp)
                                                values(@User_ID, @UserSessionGuid, @Timestamp)",
                                            new
                                            {
                                                @User_ID = UserID,
                                                @UserSessionGuid = UserSessionGUID,
                                                @Timestamp = GetUTCNowAsISO()
                                            }).FirstOrDefault();
                }

                /*
                    The native OS command below stops the USBGuard service and manually
                    sets each USB port / device to be enabled one by one. Lastly, we
                    simulate Alt+F4 keypress which closes fullscreened / kiosked Chrome thus
                    providing access to the OS UI.
                */
                RunLinuxCommand("pkexec bash -c 'systemctl stop usbguard && for i in /sys/bus/usb/devices/**/authorized_default; do echo 1 > $i; done' && xdotool key Alt+F4");

                return "200 - OK";
            }
            catch
            {
                return "500 - Internal Server Error";
            };
        }

        public IActionResult GetUtilitesLanding()
        {
            return View("UtilitiesLanding");
        }

        
        public IActionResult GetTestResults()
        {
            return View("TestResults");
        }

        public IActionResult QCLogs()
        {
            return View("QCLogs");
        }

        [HttpGet]
        public IActionResult GetTestResultsWithPagination()
        {
            var keys = Request.Query.Keys;
            foreach(var key in keys){
                Console.WriteLine(key+"     "+Request.Query[key]);
            }
            int start = Convert.ToInt32(Request.Query["start"]);
            int length = Convert.ToInt32(Request.Query["length"]);
            int draw = Convert.ToInt32(Request.Query["draw"]);
            string search = Convert.ToString(Request.Query["search[value]"]);
            
            int orderByColumn = Convert.ToInt32(Request.Query["order[0][column]"]);
            string orderDir = Convert.ToString(Request.Query["order[0][dir]"]);
            string orderByColumnName = Convert.ToString(Request.Query["columns["+orderByColumn+"][data]"]);

            //overrides for column names
            switch(orderByColumnName.ToUpper()){
                case "TESTDATE":
                    orderByColumnName = "Created";
                    break;
                default:
                    break;
            }
        
            using (var appDB = new SQLiteConnection(GetConnectionString()))
            {
                var totalRecords = appDB.Query<int>("SELECT COUNT(*) FROM TestResults").FirstOrDefault();

                //this is more efficient & faster than OFFSET keyword
                string query = @"SELECT * FROM TestResults";
                /*
                    SEARCH

                    Currently, we only have PatientID, TestDate(Created), TBV, and TBVDeviation so we will be statically coding
                    for those fields in the search.
                */
                if (search != null && search != ""){
                    search = "'%"+search+"%'";//go ahead and wrap it in quotes
                    query += " WHERE PatientID LIKE "+search;
                    query += " OR Created LIKE "+search;
                    query += " OR TBV LIKE "+search;
                    query += " OR TBVDeviation LIKE "+search;
                }
            
                //filtering and ordering here
                query += " ORDER BY "+orderByColumnName + " "+orderDir;
                query += " LIMIT " + length + " OFFSET "+start;
                var queryResults = appDB.Query<TestResult>(query);
                var results = new List<TestResultGridModel>();
                foreach(var qr in queryResults){
                    results.Add(new TestResultGridModel(qr));
                }
                return StatusCode(200, new {
                    data = results,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    draw = draw
                });
            }
        }

        [HttpGet]
        public IActionResult GetQCLogsWithPagination()
        {
            var keys = Request.Query.Keys;
            foreach(var key in keys){
                Console.WriteLine(key+"     "+Request.Query[key]);
            }
            int start = Convert.ToInt32(Request.Query["start"]);
            int length = Convert.ToInt32(Request.Query["length"]);
            int draw = Convert.ToInt32(Request.Query["draw"]);
            string search = Convert.ToString(Request.Query["search[value]"]);
            
            int orderByColumn = Convert.ToInt32(Request.Query["order[0][column]"]);
            string orderDir = Convert.ToString(Request.Query["order[0][dir]"]);
            string orderByColumnName = Convert.ToString(Request.Query["columns["+orderByColumn+"][data]"]);

            //overrides for column names
            switch(orderByColumnName.ToUpper()){
                default:
                    break;
            }
        
            using (var appDB = new SQLiteConnection(GetConnectionString()))
            {
                var totalRecords = appDB.Query<int>("SELECT COUNT(*) FROM QCLog").FirstOrDefault();

                //this is more efficient & faster than OFFSET keyword
                string query = @"SELECT * FROM QCLog";
                /*
                    SEARCH

                    Currently, we only have PatientID, TestDate(Created), TBV, and TBVDeviation so we will be statically coding
                    for those fields in the search.
                */
                if (search != null && search != ""){
                    search = "'%"+search+"%'";//go ahead and wrap it in quotes
                    query += " WHERE TestType LIKE "+search;
                    query += " OR Result LIKE "+search;
                    query += " OR Timestamp LIKE "+search;
                }
            
                //filtering and ordering here
                query += " ORDER BY "+orderByColumnName + " "+orderDir;
                query += " LIMIT " + length + " OFFSET "+start;
                var queryResults = appDB.Query<QCLog>(query);
                var results = new List<QCLogGridModel>();
                foreach(var qr in queryResults){
                    results.Add(new QCLogGridModel(qr));
                }
                return StatusCode(200, new {
                    data = results,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    draw = draw
                });
            }
        }        

        public IActionResult GetUserConfiguration(string UserSessionGUID, string PatientID)
        {
            var userConfigurationModel = new UserConfigurationViewModel();
            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                var result = conn.Query<User>("SELECT * FROM User");
                userConfigurationModel.Users = result;
            }
            userConfigurationModel.CurrentUserSession = UserSessionGUID;
            userConfigurationModel.PatientID = PatientID;

            return View("UserConfiguration", userConfigurationModel);
        }

        public IActionResult GetManualCalc()
        {
            return View("ManualCalc");
        }

        public IActionResult UpdateUser([FromBody] User user)
        {
            using (IDbConnection conn = new SQLiteConnection(GetConnectionString()))
            {
                var result = conn.Execute(@"Update User SET
                                            FirstName = @FirstName,
                                            LastName = @LastName,
                                            UserName = @UserName,
                                            Password = @Password,
                                            IsAdmin = @IsAdmin
                                            WHERE ID = @Id",
                                            new
                                            {
                                                @FirstName = user.FirstName,
                                                @LastName = user.LastName,
                                                @UserName = user.UserName,
                                                @Password = user.Password,
                                                @IsAdmin = user.IsAdmin == true ? 1 : 0,
                                                @Id = user.ID
                                            });

                return Json(result);
            }
        }
        public IActionResult AddUser([FromBody] User user)
        {
            using (IDbConnection conn = new SQLiteConnection(GetConnectionString()))
            {
                var result = conn.Execute(@"INSERT INTO User(FirstName, LastName, UserName, Password, IsAdmin)
                                            VALUES (@FirstName, @LastName, @UserName, @Password, @IsAdmin)",
                                            new
                                            {
                                                @FirstName = user.FirstName,
                                                @LastName = user.LastName,
                                                @UserName = user.UserName,
                                                @Password = user.Password,
                                                @IsAdmin = user.IsAdmin == true ? 1 : 0,
                                            });

                return Json(result);
            }
        }
        public IActionResult DeleteUser([FromBody] User user)
        {
            using (IDbConnection conn = new SQLiteConnection(GetConnectionString()))
            {
                var result = conn.Execute(@"DELETE FROM User WHERE ID = @ID",
                                            new
                                            {
                                                @ID = user.ID
                                            });

                return Json(result);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

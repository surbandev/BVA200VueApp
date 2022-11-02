using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BVA200.Models;
using static BVA200.Logic.FileExports;
using static BVA200.Logic.Common;
using System;
using System.Data.SQLite;
using Dapper;
using System.Collections.Generic;

namespace BVA200.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ILogger<ReportsController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public ActionResult SaveQCLogsToUSBDrive([FromBody] ExportModel em)
        {
            string usbPath = GetUSBFlashDrivePath();
            if (usbPath == null)
            {
                return StatusCode(500, "No flash drive detected");
            }
            if (!usbPath.EndsWith("/"))
            {
                usbPath += "/";
            }

            string filename = "";
            byte[] exportStream = null;
            try
            {
                switch (em.Format.ToUpper())
                {
                    case "XLSX":
                        return StatusCode(406, "Excel export is not available at this time.");
                    case "PDF":
                        try
                        {
                            using (var conn = new SQLiteConnection(GetConnectionString()))
                            {
                                int[] RequestedIDs = (int[])em.RequestedIDs;
                                string sql = @"
                                    SELECT ql.*, u.UserName as PreformedBy FROM QCLog ql 
                                    JOIN User u on u.ID = ql.CreatedBy";
                                if (em.AllResults == false)
                                {
                                    sql += " WHERE ql.ID IN ( ";
                                    string ids = "";
                                    foreach (int id in RequestedIDs)
                                    {
                                        ids += "," + id.ToString();
                                    }
                                    ids = ids.Trim(',').Trim();
                                    sql += ids + ")";
                                }
                                var results = conn.Query<QCLogReportModel>(sql);
                                if (results.AsList().Count == 0)
                                {
                                    throw new Exception("Failed to retrieve data from the database");
                                }
                                List<string> filenames = new List<string>();
                                foreach (QCLogReportModel ql in results)
                                {
                                    exportStream = BuildQCLogPDF(ql);
                                    filename = "BVA200-QC-LOG-" + ql.Timestamp;
                                    filename = ExportDocumentToFlashDrive(usbPath, filename, em.Format.ToLower(), exportStream);
                                    filenames.Add(filename);
                                }
                                //I know that in a loop, this will only be the last filename.
                                return StatusCode(200, filenames.ToArray());
                            }
                        }
                        catch (Exception ex)
                        {
                            System.IO.File.WriteAllText("ExportLog.txt", ex.ToString());
                            return StatusCode(500);
                        }

                    default:
                        return StatusCode(400);

                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult PrintQCLogs([FromBody] ExportModel em)
        {
            if (string.IsNullOrEmpty(em.Printer))
            {
                return StatusCode(500, "No printer was specified");
            }

            //going to do PDFs
            try
            {
                byte[] exportStream = null;
                using (var conn = new SQLiteConnection(GetConnectionString()))
                {
                    int[] RequestedIDs = (int[])em.RequestedIDs;
                    string sql = @"
                        SELECT ql.*, u.UserName as PreformedBy FROM QCLog ql 
                        JOIN User u on u.ID = ql.CreatedBy";
                    if (em.AllResults == false)
                    {
                        sql += " WHERE ql.ID IN ( ";
                        string ids = "";
                        foreach (int id in RequestedIDs)
                        {
                            ids += "," + id.ToString();
                        }
                        ids = ids.Trim(',').Trim();
                        sql += ids + ")";
                    }

                    var results = conn.Query<QCLogReportModel>(sql);
                    if (results.AsList().Count == 0)
                    {
                        throw new Exception("Failed to retrieve data from the database");
                    }
                    string fileName = Guid.NewGuid().ToString() + ".pdf";
                    foreach (QCLogReportModel qlrm in results)
                    {
                        exportStream = BuildQCLogPDF(qlrm);
                        System.IO.File.WriteAllBytes(fileName, exportStream);
                        BVA200.Logic.Common.PrintFile(fileName, em.Printer);
                        System.IO.File.Delete(fileName);
                    }

                    return StatusCode(200);
                }
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("ExportLog.txt", ex.ToString());
                return StatusCode(500);
            }

        }

        [HttpPost]
        public ActionResult PrintTestResults([FromBody] ExportModel em)
        {
            if (string.IsNullOrEmpty(em.Printer))
            {
                return StatusCode(500, "No printer was specified");
            }

            //going to do PDFs
            try
            {
                byte[] exportStream = null;
                using (var conn = new SQLiteConnection(GetConnectionString()))
                {
                    int[] RequestedIDs = (int[])em.RequestedIDs;
                    string sql = @"
                                    SELECT * FROM TestResults tr 
                                    JOIN User u on u.ID = tr.CreatedBy";
                    sql += " WHERE tr.ID IN ( ";
                    string ids = "";
                    foreach (int id in RequestedIDs)
                    {
                        ids += "," + id.ToString();
                    }
                    ids = ids.Trim(',').Trim();
                    sql += ids + ")";

                    var results = conn.Query<SingleTestResult>(sql);
                    if (results.AsList().Count == 0)
                    {
                        throw new Exception("Failed to retrieve data from the database");
                    }
                    string fileName = Guid.NewGuid().ToString() + ".pdf";
                    foreach (SingleTestResult tr in results)
                    {
                        exportStream = BuildTestResultPDF(tr);
                        System.IO.File.WriteAllBytes(fileName, exportStream);
                        BVA200.Logic.Common.PrintFile(fileName, em.Printer);
                        System.IO.File.Delete(fileName);
                    }

                    return StatusCode(200);
                }
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("ExportLog.txt", ex.ToString());
                return StatusCode(500);
            }

        }

        [HttpPost]
        public ActionResult SaveResultsToUSBDrive([FromBody] ExportModel em)
        {
            string usbPath = GetUSBFlashDrivePath();
            if (usbPath == null)
            {
                return StatusCode(500, "No flash drive detected");
            }
            if (!usbPath.EndsWith("/"))
            {
                usbPath += "/";
            }

            string filename = "";
            byte[] exportStream = null;
            try
            {

                switch (em.Format.ToUpper())
                {
                    case "XLSX":
                        exportStream = BuildTestResultsExcel(em);
                        filename = "BVA_Test_Result_Export_" + Guid.NewGuid() + "_" + DateTime.Now.ToShortDateString();
                        filename = ExportDocumentToFlashDrive(usbPath, filename, em.Format.ToLower(), exportStream);
                        return StatusCode(200, filename);
                    case "PDF":
                        try
                        {
                            using (var conn = new SQLiteConnection(GetConnectionString()))
                            {
                                int[] RequestedIDs = (int[])em.RequestedIDs;
                                string sql = @"
                                    SELECT * FROM TestResults tr 
                                    JOIN User u on u.ID = tr.CreatedBy";
                                if (em.AllResults == false)
                                {
                                    sql += " WHERE tr.ID IN ( ";
                                    string ids = "";
                                    foreach (int id in RequestedIDs)
                                    {
                                        ids += "," + id.ToString();
                                    }
                                    ids = ids.Trim(',').Trim();
                                    sql += ids + ")";
                                }
                                var results = conn.Query<SingleTestResult>(sql);
                                if (results.AsList().Count == 0)
                                {
                                    throw new Exception("Failed to retrieve data from the database");
                                }
                                List<string> filenames = new List<string>();
                                foreach (SingleTestResult tr in results)
                                {
                                    exportStream = BuildTestResultPDF(tr);
                                    var testDate = tr.Created;
                                    testDate = testDate == null ? DateTime.Now : testDate;
                                    string shortDateString = Convert.ToDateTime(testDate).ToShortDateString();
                                    string longTimeString = Convert.ToDateTime(testDate).ToLongTimeString() + "_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                    filename = "";
                                    var patientID = tr.PatientID;
                                    var format = em.Format.ToLower();
                                    if (patientID != "")
                                    {
                                        filename = "BVA200_Export_" + patientID + "_" + shortDateString + "_" + longTimeString + "." + format;
                                    }
                                    else
                                    {
                                        filename = "BVA200_Export_" + shortDateString + "_" + longTimeString + "." + format;
                                    }

                                    filename = ExportDocumentToFlashDrive(usbPath, filename, em.Format, exportStream);
                                    filenames.Add(filename);
                                }
                                //I know that in a loop, this will only be the last filename.
                                return StatusCode(200, filenames.ToArray());
                            }
                        }
                        catch (Exception ex)
                        {
                            System.IO.File.WriteAllText("ExportLog.txt", ex.ToString());
                            return StatusCode(500);
                        }

                    default:
                        return StatusCode(400);

                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

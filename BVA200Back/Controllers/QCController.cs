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
    public class QCController : Controller
    {
        private readonly ILogger<QCController> _logger;

        public QCController(ILogger<QCController> logger)
        {
            _logger = logger;
        }

        public IActionResult QCCalibration()
        {

            var detectorSettings = new DetectorSettings();
            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                var result = conn.Query<DetectorSettings>("SELECT * FROM DetectorSettings WHERE Detector = 'Topaz'").FirstOrDefault();
                detectorSettings.CountTime = result.CountTime;
                detectorSettings.CountBGTime = result.CountBGTime;
                detectorSettings.SampleVol = result.SampleVol;
                detectorSettings.TracerDisseminationTime = result.TracerDisseminationTime;
                detectorSettings.CoarseGain = result.CoarseGain;
                detectorSettings.FineGain = result.FineGain;
                detectorSettings.LLD = result.LLD;

                return View("QCCalibration", detectorSettings);
            }
        }

        public IActionResult QCConstancy()
        {

            var qcConstancyModel = new QCModel();
            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                var result = conn.Query<DetectorSettings>("SELECT * FROM DetectorSettings WHERE Detector = 'Topaz'").FirstOrDefault();
                var constStore = conn.Query<QCLog>("SELECT CPM, Timestamp FROM QCLog WHERE TestType = 'Constancy' ORDER BY ID DESC").FirstOrDefault();
                qcConstancyModel.CountTime = result.CountTime;
                qcConstancyModel.CountBGTime = result.CountBGTime;
                qcConstancyModel.SampleVol = result.SampleVol;
                qcConstancyModel.TracerDisseminationTime = result.TracerDisseminationTime;
                qcConstancyModel.CoarseGain = result.CoarseGain;
                qcConstancyModel.FineGain = result.FineGain;
                qcConstancyModel.LLD = result.LLD;
                qcConstancyModel.Timestamp = constStore.Timestamp;
                qcConstancyModel.CPM = constStore.CPM;

                return View("QCConstancy", qcConstancyModel);
            }
        }

        public IActionResult QCEfficiency()
        {

            var qcEfficiencyModel = new QCModel();
            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                var result = conn.Query<DetectorSettings>("SELECT * FROM DetectorSettings WHERE Detector = 'Topaz'").FirstOrDefault();
                var constStore = conn.Query<QCLog>("SELECT CPM, Timestamp FROM QCLog WHERE TestType = 'Efficiency' ORDER BY ID DESC").FirstOrDefault();
                if(constStore != null ){
                    qcEfficiencyModel.Timestamp = constStore.Timestamp;
                    qcEfficiencyModel.CPM = constStore.CPM;
                }else{
                    qcEfficiencyModel.Timestamp = null;
                    qcEfficiencyModel.CPM = null;
                }
                qcEfficiencyModel.CountTime = result.CountTime;
                qcEfficiencyModel.CountBGTime = result.CountBGTime;
                qcEfficiencyModel.SampleVol = result.SampleVol;
                qcEfficiencyModel.TracerDisseminationTime = result.TracerDisseminationTime;
                qcEfficiencyModel.CoarseGain = result.CoarseGain;
                qcEfficiencyModel.FineGain = result.FineGain;
                qcEfficiencyModel.LLD = result.LLD;

                return View("QCEfficiency", qcEfficiencyModel);
            }
        }

        public IActionResult QCLinearity()
        {
            var qcLinearityModel = new QCModel();
            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                var result = conn.Query<DetectorSettings>("SELECT * FROM DetectorSettings WHERE Detector = 'Topaz'").FirstOrDefault();
                var constStore = conn.Query<QCLog>("SELECT CPM, Timestamp FROM QCLog WHERE TestType = 'Constancy' ORDER BY ID DESC").FirstOrDefault();
                qcLinearityModel.CountTime = result.CountTime;
                qcLinearityModel.CountBGTime = result.CountBGTime;
                qcLinearityModel.SampleVol = result.SampleVol;
                qcLinearityModel.TracerDisseminationTime = result.TracerDisseminationTime;
                qcLinearityModel.CoarseGain = result.CoarseGain;
                qcLinearityModel.FineGain = result.FineGain;
                qcLinearityModel.LLD = result.LLD;
                qcLinearityModel.Timestamp = constStore.Timestamp;
                qcLinearityModel.CPM = constStore.CPM;

                return View("QCLinearity", qcLinearityModel);
            }
        }

        public IActionResult QCLanding(Guid UserSessionGUID)
        {
            using (var conn = new SQLiteConnection(GetConnectionString()))
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
                
                return View("QCLanding", model);
            }
        }

        [HttpPost]
        public ActionResult SaveFineGainCalibration([FromBody] DetectorSettings settings)
        {
            try
            {
                using (IDbConnection conn = new SQLiteConnection(GetConnectionString()))
                {
                    var results = conn.Execute(@"UPDATE DetectorSettings SET 
                                            FineGain = @FineGain 
                                            WHERE Detector = @Detector"
                                            , new
                                            {
                                                @FineGain = settings.FineGain,
                                                @Detector = settings.Detector
                                            });

                    return StatusCode(200, results);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost]
        public ActionResult SaveQCLog([FromHeader] int UserID, [FromBody] QCLog settings)
        {
            try
            {
                int createdBy = UserID;
                int updatedBy = createdBy;
                String created = GetUTCNowAsISO();
                String updated = created;

                using (IDbConnection conn = new SQLiteConnection(GetConnectionString()))
                {
                    var results = conn.Execute(@"INSERT INTO QCLog(TestType, Timestamp, Counts, CPM, Result, CreatedBy, UpdatedBy, Created, Updated)
                                                VALUES(@TestType, @Timestamp, @Counts, @CPM, @Result, @CreatedBy, @UpdatedBy, @Created, @Updated)"
                                            , new
                                            {
                                                @TestType = settings.TestType,
                                                @Timestamp = settings.Timestamp,
                                                @Counts = settings.Counts,
                                                @CPM = settings.CPM,
                                                @Result = settings.Result,
                                                @CreatedBy = createdBy,
                                                @UpdatedBy = updatedBy,
                                                @Created = created,
                                                @Updated = updated
                                            });

                    return StatusCode(200, results);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost]
        public ActionResult SaveQC([FromBody] QCLog qc)
        {
            try
            {
                using (IDbConnection conn = new SQLiteConnection(GetConnectionString()))
                {
                    var results = conn.Execute(@"INSERT OR REPLACE INTO QCLog(ID, TestType, Timestamp, Counts, CPM)
                                                VALUES((SELECT ID FROM QCLog WHERE TestType = @TestType),
                                                   @TestType,
                                                   @Timestamp,
                                                   @Counts,
                                                   @CPM);"
                                            , new
                                            {
                                                @TestType = qc.TestType,
                                                @Timestamp = qc.Timestamp,
                                                @Counts = qc.Counts,
                                                @CPM = qc.CPM
                                            });

                    return StatusCode(200, results);
                }
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using BVA200.Models;
using static BVA200.Logic.Common;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BVA200.Controllers
{
    public class BVAController : Controller
    {
        private readonly ILogger<BVAController> _logger;
        public BVAController(ILogger<BVAController> logger)
        {
            _logger = logger;
        }

        public IActionResult GetIdealBloodVolume([FromBody] Patient patient)
        {
            using (IDbConnection conn = new SQLiteConnection(GetConnectionString()))
            {
                // Need to take in sex and decide if male calc is needed.
                var LowIdealWeightHeight = conn.Query<IdealWeightHeight>("SELECT Height, IdealWeight FROM IdealWeightHeight WHERE Height <=  @Height ORDER BY Height DESC",
                            new { @Height = patient.Height }).FirstOrDefault();
                var HighIdealWeightHeight = conn.Query<IdealWeightHeight>("SELECT Height, IdealWeight FROM IdealWeightHeight WHERE Height >=  @Height",
                            new { @Height = patient.Height }).FirstOrDefault();
                decimal IdealWt = LowIdealWeightHeight.IdealWeight + (HighIdealWeightHeight.IdealWeight - LowIdealWeightHeight.IdealWeight) * (patient.Height - LowIdealWeightHeight.Height) / (HighIdealWeightHeight.Height - LowIdealWeightHeight.Height);
                decimal ConvertedIdealWt = IdealWt;
                if (patient.Sex == "male")
                {
                    ConvertedIdealWt = IdealWt * (decimal)1.08;
                }
                var WeightDeviationPercentage = ((patient.Weight - ConvertedIdealWt) / ConvertedIdealWt) * 100;

                var LowVF = conn.Query<VolumeFactor>("SELECT WeightDeviationPercent, VolumeFactorMlKg FROM VolumeFactor WHERE WeightDeviationPercent <=  @WeightDeviationPercent ORDER BY WeightDeviationPercent DESC",
                            new { @WeightDeviationPercent = WeightDeviationPercentage }).FirstOrDefault();
                var HighVF = conn.Query<VolumeFactor>("SELECT WeightDeviationPercent, VolumeFactorMlKg FROM VolumeFactor WHERE WeightDeviationPercent >=  @WeightDeviationPercent",
                            new { @WeightDeviationPercent = WeightDeviationPercentage }).FirstOrDefault();

                var VF = LowVF.VolumeFactorMlKg + (HighVF.VolumeFactorMlKg - LowVF.VolumeFactorMlKg) * (WeightDeviationPercentage - LowVF.WeightDeviationPercent) / (HighVF.WeightDeviationPercent - LowVF.WeightDeviationPercent);

                var IdealTotalBloodVolume = VF * patient.Weight;
                var Converted = Convert.ToInt32(IdealTotalBloodVolume);

                return Json(Converted);
            };
        }

        [HttpPost]
        public ActionResult LoadTestData([FromBody] LoadTestResultModel payload)
        {
            int requestedID = payload.TestResultID;
            try
            {
                using (var conn = new SQLiteConnection(GetConnectionString()))
                {
                    var result = conn.Query<TestResult>("SELECT * FROM TestResults WHERE ID = " + requestedID).FirstOrDefault();
                    return StatusCode(200, result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public IActionResult SaveTestResults([FromHeader] int UserID, [FromBody] TestResult payload)
        {
            int createdBy = UserID;
            int updatedBy = createdBy;
            String created = GetUTCNowAsISO();
            String updated = created;

            var dbPayload = new
            {
                PatientID = payload.PatientID,
                Weight = payload.Weight,
                Height = payload.Height,
                Sex = payload.Sex,
                Age = payload.Age,
                OrderingPhysician = payload.OrderingPhysician,
                WholeBlood = payload.WholeBlood || false,
                Amputee = payload.Amputee,
                AmputeeCorrectionPercentage = payload.AmputeeCorrectionPercentage,
                Pregnant = payload.Pregnant,
                InjectateLotNumber = payload.InjectateLotNumber,
                StandardStrength = payload.StandardStrength,
                InjectionTimestamp = payload.InjectionTimestamp,
                BackgroundCountMins = payload.BackgroundCountMins,
                TracerDisMins = payload.TracerDisMins,
                SampleCountMins = payload.SampleCountMins,
                AlbuminTransudationRate = payload.AlbuminTransudationRate,
                PHCTAVG = payload.PHCTAVG,
                NHCTAVG = payload.NHCTAVG,
                UBV = payload.UBV,
                IBV = payload.IBV,
                TBV = payload.TBV,
                TBVDeviation = payload.TBVDeviation,
                IPV = payload.IPV,
                PV = payload.PV,
                PVDeviation = payload.PVDeviation,
                IRBCV = payload.IRBCV,
                RBCV = payload.RBCV,
                RBCVDeviation = payload.RBCVDeviation,

                LowChannel = payload.LowChannel,
                HighChannel = payload.HighChannel,

                //may need to do a Json Stringify or similar here
                BackgroundSpectrum = JsonConvert.SerializeObject(payload.BackgroundSpectrum ?? new int[] { }),
                BackgroundCount = payload.BackgroundCount,
                BaselineSpectrum = JsonConvert.SerializeObject(payload.BaselineSpectrum ?? new int[] { }),
                BaselineCount = payload.BaselineCount,
                StandardSpectrum = JsonConvert.SerializeObject(payload.StandardSpectrum ?? new int[] { }),
                StandardCount = payload.StandardCount,
                PostInjection1DrawnTimestamp = payload.PostInjection1DrawnTimestamp,
                PostInjection1CountedTimestamp = payload.PostInjection1CountedTimestamp,
                PostInjection1Spectrum = JsonConvert.SerializeObject(payload.PostInjection1Spectrum ?? new int[] { }),
                PostInjection1Count = payload.PostInjection1Count,
                PostInjection1PHCT = payload.PostInjection1PHCT,
                PostInjection1NHCT = payload.PostInjection1NHCT,
                PostInjection1Included = payload.PostInjection1Included,
                PostInjection2DrawnTimestamp = payload.PostInjection2DrawnTimestamp,
                PostInjection2CountedTimestamp = payload.PostInjection2CountedTimestamp,
                PostInjection2Spectrum = JsonConvert.SerializeObject(payload.PostInjection2Spectrum ?? new int[] { }),
                PostInjection2Count = payload.PostInjection2Count,
                PostInjection2PHCT = payload.PostInjection2PHCT,
                PostInjection2NHCT = payload.PostInjection2NHCT,
                PostInjection2Included = payload.PostInjection2Included,
                PostInjection3DrawnTimestamp = payload.PostInjection3DrawnTimestamp,
                PostInjection3CountedTimestamp = payload.PostInjection3CountedTimestamp,
                PostInjection3Spectrum = JsonConvert.SerializeObject(payload.PostInjection3Spectrum ?? new int[] { }),
                PostInjection3Count = payload.PostInjection3Count,
                PostInjection3PHCT = payload.PostInjection3PHCT,
                PostInjection3NHCT = payload.PostInjection3NHCT,
                PostInjection3Included = payload.PostInjection3Included,
                PostInjection4DrawnTimestamp = payload.PostInjection4DrawnTimestamp,
                PostInjection4CountedTimestamp = payload.PostInjection4CountedTimestamp,
                PostInjection4Spectrum = JsonConvert.SerializeObject(payload.PostInjection4Spectrum ?? new int[] { }),
                PostInjection4Count = payload.PostInjection4Count,
                PostInjection4PHCT = payload.PostInjection4PHCT,
                PostInjection4NHCT = payload.PostInjection4NHCT,
                PostInjection4Included = payload.PostInjection4Included,
                PostInjection5DrawnTimestamp = payload.PostInjection5DrawnTimestamp,
                PostInjection5CountedTimestamp = payload.PostInjection5CountedTimestamp,
                PostInjection5Spectrum = JsonConvert.SerializeObject(payload.PostInjection5Spectrum ?? new int[] { }),
                PostInjection5Count = payload.PostInjection5Count,
                PostInjection5PHCT = payload.PostInjection5PHCT,
                PostInjection5NHCT = payload.PostInjection5NHCT,
                PostInjection5Included = payload.PostInjection5Included,
                TestType = payload.TestType,
                CreatedBy = createdBy,
                UpdatedBy = updatedBy,
                Created = created,
                Updated = updated
            };
            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                var result = conn.Execute(@"INSERT INTO TestResults (
                    PatientID, Weight, Height, Sex, Age, OrderingPhysician, WholeBlood, Amputee, AmputeeCorrectionPercentage,
                    Pregnant, InjectateLotNumber, StandardStrength, InjectionTimestamp, BackgroundCountMins, TracerDisMins, SampleCountMins, AlbuminTransudationRate, 
                    PHCTAVG, NHCTAVG, UBV, IBV, TBV, 
                    TBVDeviation, IPV, PV, PVDeviation, IRBCV, RBCV, RBCVDeviation, LowChannel, HighChannel, BackgroundSpectrum, BackgroundCount, 
                    BaselineSpectrum, BaselineCount, StandardSpectrum, StandardCount, 
                    PostInjection1DrawnTimestamp, PostInjection1CountedTimestamp, PostInjection1Spectrum, PostInjection1Count, PostInjection1PHCT, PostInjection1NHCT, PostInjection1Included,
                    PostInjection2DrawnTimestamp, PostInjection2CountedTimestamp, PostInjection2Spectrum, PostInjection2Count, PostInjection2PHCT, PostInjection2NHCT, PostInjection2Included,
                    PostInjection3DrawnTimestamp, PostInjection3CountedTimestamp, PostInjection3Spectrum, PostInjection3Count, PostInjection3PHCT, PostInjection3NHCT, PostInjection3Included,
                    PostInjection4DrawnTimestamp, PostInjection4CountedTimestamp, PostInjection4Spectrum, PostInjection4Count, PostInjection4PHCT, PostInjection4NHCT, PostInjection4Included,
                    PostInjection5DrawnTimestamp, PostInjection5CountedTimestamp, PostInjection5Spectrum, PostInjection5Count, PostInjection5PHCT, PostInjection5NHCT, PostInjection5Included,
                    TestType, CreatedBy, UpdatedBy, Created, Updated
                    )VALUES(
                    @PatientID, @Weight, @Height, @Sex, @Age, @OrderingPhysician, @WholeBlood, @Amputee, @AmputeeCorrectionPercentage,
                    @Pregnant, @InjectateLotNumber, @StandardStrength, @InjectionTimestamp, @BackgroundCountMins, @TracerDisMins, @SampleCountMins, @AlbuminTransudationRate, 
                    @PHCTAVG, @NHCTAVG, @UBV, @IBV, @TBV, 
                    @TBVDeviation, @IPV, @PV, @PVDeviation, @IRBCV, @RBCV, @RBCVDeviation, @LowChannel, @HighChannel, @BackgroundSpectrum, @BackgroundCount, 
                    @BaselineSpectrum, @BaselineCount, @StandardSpectrum, @StandardCount,
                    @PostInjection1DrawnTimestamp, @PostInjection1CountedTimestamp, @PostInjection1Spectrum, @PostInjection1Count, @PostInjection1PHCT, @PostInjection1NHCT, @PostInjection1Included,
                    @PostInjection2DrawnTimestamp, @PostInjection2CountedTimestamp, @PostInjection2Spectrum, @PostInjection2Count, @PostInjection2PHCT, @PostInjection2NHCT, @PostInjection2Included,
                    @PostInjection3DrawnTimestamp, @PostInjection3CountedTimestamp, @PostInjection3Spectrum, @PostInjection3Count, @PostInjection3PHCT, @PostInjection3NHCT, @PostInjection3Included,
                    @PostInjection4DrawnTimestamp, @PostInjection4CountedTimestamp, @PostInjection4Spectrum, @PostInjection4Count, @PostInjection4PHCT, @PostInjection4NHCT, @PostInjection4Included,
                    @PostInjection5DrawnTimestamp, @PostInjection5CountedTimestamp, @PostInjection5Spectrum, @PostInjection5Count, @PostInjection5PHCT, @PostInjection5NHCT, @PostInjection5Included,
                    @TestType, @CreatedBy, @UpdatedBy, @Created, @Updated
                    )", dbPayload);

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

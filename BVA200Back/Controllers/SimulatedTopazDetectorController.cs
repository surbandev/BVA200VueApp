using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using BVA200.Models;
using static BVA200.Logic.Common;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BVA200.Controllers
{
    public class SimulatedTopazDetectorController : Controller
    {
        #region Properties
        public const int DEF_STRING_SIZE = 256;
        int iDeviceType = 0;
        StringBuilder szDevicePath = new StringBuilder(256);
        StringBuilder szDeviceName = new StringBuilder(256);
        StringBuilder szSerial = new StringBuilder(256);
        public static IntPtr hMCA;

        //DO NOT CHANGE THE NEXT TWO VALUES!!! THEY MUST BE 4096!!!
        private static UInt32[] m_aiSpectrum = new UInt32[4096];   // Spectrum
        private UInt32 m_iNrOfChannels = 4096;              // Nr of channels
        private static DetectorState detectorState = DetectorState.Closed;

        private static Boolean requestInProcess = false;
        private static SpectrumRead lastRead = null;

        #endregion

        #region Initial view
        public IActionResult SetupDetector()
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

                return View("Detector", detectorSettings);
            }
        }
        #endregion

        #region Device I/O
        [HttpPost]
        public ActionResult OpenTopazReader()
        {
            HttpContext.Session.SetString("DetectorState", DetectorState.Idle.ToString());
            return StatusCode(200, "Devices: 1 OpenedState: Successful");
        }

        [HttpPost]
        public ActionResult CloseTopazReader()
        {
            try
            {
                HttpContext.Session.SetString("DetectorState", DetectorState.Closed.ToString());
                return StatusCode(200);
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost]
        public ActionResult StartRead()
        {
            try
            {
                string status = "Acquiring";
                HttpContext.Session.SetString("DetectorState", DetectorState.Reading.ToString());
                return StatusCode(200, status);
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost]
        public ActionResult StopRead()
        {
            try
            {
                HttpContext.Session.SetString("DetectorState", DetectorState.Idle.ToString());
                return StatusCode(200, "Stopped");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost]
        public ActionResult SetDetectorDefaultParameters()
        {
            try
            {
                return StatusCode(200, "Reset");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost]
        public ActionResult ClearMemory()
        {
            try
            {
                HttpContext.Session.SetString("DetectorState", DetectorState.Idle.ToString());
                return StatusCode(200, "Cleared");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        #endregion

        #region Device Logic
        [HttpPost]
        public ActionResult ReadSpectrum()
        {
            try
            {
                SpectrumRead spectrumRead = new SpectrumRead();

                int iGainFine = 2048, iGainCoarse = 1, iGainDigital = 0, iPHAL = 308, iPHAH = 420, iCurrCount = 0, iHVstatus = 0;
                float sElapsedRT = 0, sElapsedLT = 0, sHVvalue = 1000;

                spectrumRead.HighChannel = iPHAH;
                spectrumRead.LowChannel = iPHAL;
                spectrumRead.FineGain = iGainFine;
                spectrumRead.CoarseGain = iGainCoarse;
                spectrumRead.DigitalGain = iGainDigital;
                spectrumRead.CurrentCounts = iCurrCount;


                string txtLivetime = sElapsedLT.ToString();
                string txtRealtime = sElapsedRT.ToString();

                // Set live time and real time
                spectrumRead.LiveTime = sElapsedLT;
                spectrumRead.RealTime = sElapsedRT;
                spectrumRead.LiveTimeString = txtLivetime;
                spectrumRead.RealTimeString = txtRealtime;
                spectrumRead.HighVoltage = sHVvalue;
                spectrumRead.HighVoltageStatus = iHVstatus;

                uint[] backgroundSpectrum = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 0, 0, 0, 1, 0, 0, 4, 1, 2, 1, 2, 3, 5, 2, 3, 1, 0, 2, 4, 2, 1, 1, 2, 3, 2, 2, 4, 0, 5, 4, 5, 4, 3, 3, 4, 3, 4, 14, 1, 11, 7, 8, 3, 5, 4, 7, 6, 5, 5, 9, 9, 7, 11, 8, 11, 8, 8, 6, 13, 10, 7, 8, 11, 12, 9, 10, 15, 8, 11, 10, 10, 8, 6, 5, 16, 13, 6, 12, 10, 11, 9, 10, 11, 8, 14, 11, 14, 6, 7, 9, 11, 10, 4, 13, 12, 7, 11, 11, 5, 5, 14, 8, 8, 7, 8, 14, 8, 2, 13, 10, 14, 13, 10, 10, 7, 10, 15, 9, 6, 10, 11, 12, 11, 8, 12, 4, 7, 10, 10, 13, 9, 11, 13, 10, 7, 11, 6, 7, 10, 7, 7, 9, 8, 8, 8, 6, 8, 6, 13, 12, 8, 10, 12, 5, 6, 10, 6, 8, 6, 4, 10, 4, 7, 13, 8, 4, 4, 8, 7, 11, 5, 8, 5, 5, 13, 8, 5, 5, 3, 10, 9, 6, 5, 9, 10, 6, 5, 6, 6, 5, 5, 4, 3, 8, 4, 8, 6, 5, 7, 9, 3, 3, 7, 2, 3, 7, 8, 7, 8, 2, 5, 6, 9, 2, 4, 4, 5, 6, 5, 10, 4, 5, 4, 3, 4, 4, 4, 3, 3, 4, 3, 10, 4, 2, 8, 4, 3, 3, 6, 6, 7, 5, 5, 6, 2, 4, 1, 0, 6, 3, 2, 1, 7, 4, 6, 5, 8, 9, 3, 8, 3, 3, 5, 10, 7, 2, 5, 2, 0, 4, 4, 3, 3, 5, 4, 3, 1, 7, 2, 1, 6, 2, 4, 1, 3, 0, 2, 1, 5, 1, 4, 4, 1, 1, 6, 3, 6, 3, 5, 3, 1, 2, 2, 2, 4, 2, 3, 3, 3, 3, 4, 4, 4, 2, 6, 2, 0, 4, 3, 2, 2, 3, 3, 3, 1, 5, 3, 2, 1, 1, 2, 1, 3, 2, 1, 4, 2, 3, 2, 3, 5, 2, 2, 7, 1, 2, 0, 2, 3, 3, 3, 1, 2, 4, 1, 1, 3, 1, 2, 4, 1, 0, 2, 2, 4, 2, 3, 2, 2, 6, 2, 1, 1, 2, 4, 1, 1, 2, 1, 5, 4, 3, 1, 2, 0, 3, 3, 1, 1, 2, 5, 1, 6, 0, 1, 1, 3, 4, 3, 1, 4, 2, 1, 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 2, 2, 3, 3, 0, 0, 1, 2, 0, 1, 2, 3, 1, 2, 2, 3, 1, 2, 3, 1, 1, 2, 1, 5, 1, 3, 0, 1, 0, 3, 5, 0, 2, 0, 1, 4, 1, 0, 2, 2, 1, 0, 0, 2, 2, 0, 0, 1, 1, 0, 1, 0, 0, 0, 1, 2, 2, 1, 1, 1, 0, 1, 2, 0, 1, 2, 2, 0, 1, 3, 3, 2, 0, 1, 2, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 2, 0, 2, 2, 1, 1, 0, 2, 1, 0, 3, 1, 0, 1, 0, 1, 1, 1, 0, 2, 0, 0, 1, 0, 2, 0, 0, 0, 0, 1, 1, 1, 0, 2, 0, 1, 1, 2, 0, 1, 0, 0, 0, 1, 1, 0, 1, 0, 2, 2, 0, 1, 1, 0, 1, 1, 2, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 1, 0, 1, 2, 0, 1, 0, 0, 0, 0, 3, 1, 2, 1, 0, 1, 2, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 3, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 2, 1, 0, 0, 0, 0, 4, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 2, 0, 0, 1, 0, 0, 0, 1, 0, 3, 1, 2, 2, 0, 0, 1, 1, 2, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 0, 0, 2, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 0, 2, 0, 2, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 2, 0, 1, 1, 0, 0, 2, 1, 3, 0, 0, 0, 0, 1, 1, 1, 2, 2, 1, 2, 1, 0, 1, 2, 0, 0, 0, 1, 1, 0, 1, 0, 1, 0, 1, 2, 0, 1, 0, 0, 0, 1, 2, 0, 1, 1, 0, 0, 1, 0, 0, 2, 0, 1, 0, 2, 0, 2, 0, 0, 1, 3, 1, 0, 2, 0, 0, 2, 1, 1, 0, 2, 0, 1, 1, 1, 0, 2, 0, 0, 0, 2, 1, 1, 0, 1, 2, 1, 1, 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 1, 2, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 1, 3, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 3, 0, 0, 0, 0 };
                uint[] standardSpectrum = new uint[] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 2, 1, 2, 0, 0, 4, 7, 8, 5, 13, 13, 19, 26, 25, 40, 53, 47, 60, 66, 78, 72, 74, 105, 90, 111, 93, 107, 97, 80, 84, 69, 75, 59, 55, 46, 55, 50, 42, 32, 37, 34, 46, 30, 26, 28, 33, 26, 29, 37, 22, 21, 23, 24, 35, 24, 27, 25, 28, 33, 38, 34, 37, 39, 34, 30, 29, 34, 32, 30, 20, 21, 31, 22, 32, 45, 31, 32, 42, 44, 26, 36, 33, 40, 29, 47, 39, 35, 31, 31, 40, 34, 35, 37, 28, 38, 34, 22, 33, 40, 34, 34, 34, 30, 39, 38, 20, 45, 26, 31, 38, 33, 33, 37, 32, 32, 25, 32, 27, 30, 29, 36, 39, 33, 41, 25, 29, 34, 22, 25, 42, 29, 44, 30, 32, 27, 40, 41, 27, 34, 25, 46, 28, 25, 41, 32, 29, 38, 41, 33, 38, 28, 46, 33, 27, 26, 34, 24, 30, 28, 28, 29, 29, 27, 20, 27, 39, 29, 23, 32, 39, 33, 23, 39, 35, 40, 27, 39, 34, 23, 28, 29, 29, 27, 30, 37, 31, 22, 25, 34, 30, 31, 27, 48, 30, 34, 46, 29, 27, 30, 35, 43, 37, 26, 32, 36, 30, 33, 30, 34, 37, 38, 27, 33, 29, 37, 44, 38, 33, 34, 38, 38, 42, 46, 31, 35, 35, 51, 36, 39, 28, 42, 44, 39, 35, 44, 34, 38, 38, 36, 43, 44, 36, 34, 44, 36, 34, 29, 34, 35, 36, 34, 41, 35, 35, 35, 42, 25, 24, 26, 37, 33, 37, 39, 44, 31, 27, 27, 29, 36, 27, 27, 27, 39, 36, 39, 31, 23, 34, 39, 30, 25, 28, 27, 27, 27, 35, 30, 29, 24, 32, 33, 24, 31, 26, 27, 29, 29, 32, 29, 33, 22, 33, 29, 35, 19, 35, 26, 18, 24, 29, 28, 24, 22, 31, 23, 27, 28, 34, 25, 34, 27, 36, 23, 24, 22, 21, 30, 33, 26, 27, 30, 29, 28, 26, 25, 33, 36, 25, 24, 22, 31, 42, 32, 26, 26, 24, 22, 27, 26, 27, 26, 22, 30, 16, 29, 22, 13, 15, 17, 26, 19, 22, 30, 24, 19, 25, 22, 18, 30, 22, 15, 29, 23, 28, 28, 16, 18, 18, 20, 25, 30, 15, 28, 24, 31, 22, 20, 28, 23, 24, 26, 27, 25, 23, 26, 18, 28, 22, 26, 31, 27, 11, 18, 26, 22, 23, 24, 17, 20, 18, 23, 25, 18, 21, 20, 31, 23, 18, 18, 23, 28, 20, 22, 12, 24, 12, 14, 12, 24, 20, 16, 18, 18, 19, 37, 26, 16, 15, 14, 19, 22, 19, 29, 16, 15, 20, 18, 15, 24, 23, 20, 15, 19, 24, 21, 12, 21, 27, 22, 10, 18, 17, 16, 17, 11, 17, 17, 20, 9, 12, 17, 15, 16, 17, 14, 9, 18, 9, 17, 14, 20, 18, 14, 18, 20, 15, 15, 13, 20, 17, 10, 20, 8, 19, 14, 14, 11, 20, 15, 16, 10, 13, 22, 13, 17, 14, 12, 14, 11, 8, 16, 14, 11, 14, 20, 15, 10, 14, 10, 11, 13, 17, 4, 11, 14, 5, 8, 6, 17, 11, 11, 12, 14, 7, 10, 9, 11, 8, 8, 10, 8, 17, 13, 11, 13, 17, 16, 9, 11, 7, 6, 13, 15, 10, 11, 10, 6, 15, 7, 13, 8, 6, 6, 8, 8, 11, 6, 12, 12, 7, 13, 12, 10, 11, 6, 4, 4, 12, 5, 2, 5, 9, 13, 5, 6, 11, 6, 5, 1, 11, 6, 9, 4, 4, 6, 3, 3, 12, 9, 5, 7, 6, 6, 8, 5, 9, 5, 1, 4, 7, 3, 2, 7, 3, 2, 6, 2, 3, 8, 2, 1, 3, 7, 3, 6, 2, 9, 3, 3, 3, 7, 5, 6, 2, 2, 5, 4, 3, 4, 7, 3, 5, 4, 3, 5, 6, 4, 1, 1, 7, 2, 5, 5, 1, 5, 1, 4, 5, 2, 2, 6, 4, 2, 2, 3, 1, 2, 6, 2, 3, 3, 4, 1, 4, 0, 1, 4, 4, 7, 7, 4, 5, 5, 1, 4, 4, 1, 5, 4, 3, 2, 4, 1, 2, 3, 2, 1, 4, 4, 3, 4, 3, 1, 6, 4, 2, 6, 4, 7, 2, 3, 2, 3, 4, 4, 2, 5, 3, 1, 4, 6, 5, 4, 5, 7, 5, 8, 3, 6, 8, 9, 5, 12, 8, 13, 15, 11, 9, 21, 11, 10, 23, 15, 23, 11, 21, 22, 19, 14, 24, 29, 30, 40, 26, 28, 28, 33, 32, 31, 45, 29, 38, 50, 52, 47, 44, 59, 59, 53, 57, 67, 75, 60, 75, 70, 77, 62, 67, 86, 85, 89, 88, 79, 99, 96, 79, 88, 90, 93, 88, 112, 112, 112, 112, 119, 109, 96, 112, 117, 110, 125, 128, 97, 116, 111, 115, 140, 128, 125, 111, 119, 114, 120, 107, 109, 104, 125, 123, 104, 115, 88, 100, 121, 89, 99, 100, 106, 103, 94, 82, 94, 94, 90, 87, 77, 83, 79, 72, 72, 54, 80, 62, 58, 65, 58, 69, 71, 67, 64, 64, 51, 53, 52, 55, 45, 42, 44, 51, 34, 30, 42, 36, 35, 25, 39, 30, 33, 27, 22, 27, 22, 24, 24, 16 };

                uint[] spectrum = standardSpectrum;

                spectrumRead.DetectorSpectrum = spectrum.ToList();
                spectrumRead.Peaks = new CanberraPeakFinder().FindPeaks(spectrum);
                if (spectrumRead.Peaks.Count() < 1)
                {
                    spectrumRead.Peaks = null;
                }
                return StatusCode(200, spectrumRead);
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
            finally
            {
                requestInProcess = false;
            }
        }

        [HttpPost]
        public ActionResult SetDetectorParameters([FromBody] DetectorSettings settings)
        {
            try
            {
                return StatusCode(200, 0);
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        #endregion

        #region Database Logic
        [HttpPost]
        public ActionResult SaveDetectorSettings([FromBody] DetectorSettings settings)
        {
            try
            {
                using (IDbConnection conn = new SQLiteConnection(GetConnectionString()))
                {
                    var results = conn.Execute(@"UPDATE DetectorSettings SET 
                                            FineGain = @FineGain, 
                                            CoarseGain = @CoarseGain,
                                            CountBGTime = @CountBGTime,
                                            CountLimit = @CountLimit,
                                            CountTime = @CountTime,
                                            SampleVol = @SampleVol,
                                            TracerDisseminationTime = @TracerDisseminationTime                                       
                                            WHERE Detector = @Detector"
                                            , new
                                            {
                                                @FineGain = settings.FineGain,
                                                @CoarseGain = settings.CoarseGain,
                                                @CountBGTime = settings.CountBGTime,
                                                @CountLimit = settings.CountLimit,
                                                @CountTime = settings.CountTime,
                                                @SampleVol = settings.SampleVol,
                                                @TracerDisseminationTime = settings.TracerDisseminationTime,
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
        public ActionResult SaveCalibration([FromBody] CalibrationLog cLog)
        {
            try
            {
                using (IDbConnection conn = new SQLiteConnection(GetConnectionString()))
                {
                    var results = conn.Execute(@"INSERT INTO CalibrationLog (Timestamp, Standard, Peaks) VALUES
                                            (@Timestamp, @Standard, @StoredPeaks)"
                                            , new
                                            {
                                                @Timestamp = cLog.Timestamp,
                                                @Standard = cLog.Standard,
                                                @StoredPeaks = cLog.StoredPeaks
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
        public ActionResult SaveDetectorCountForTime([FromBody] DetectorSettings settings)
        {
            try
            {
                using (IDbConnection conn = new SQLiteConnection(GetConnectionString()))
                {
                    var results = conn.Execute(@"UPDATE DetectorSettings SET 
                                                CountBackgroundTime = @CountBackgroundTime                                            
                                                WHERE Detector = @Detector"
                                            , new
                                            {
                                                @CountBackgroundTime = settings.CountBGTime,
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

        [HttpGet]
        public ActionResult GetDetectorState()
        {
            var detectorState = HttpContext.Session.GetString("DetectorState");
            if (detectorState == null || detectorState == "")
            {
                detectorState = "Closed";
            }
            return StatusCode(200, detectorState);
        }

        [HttpPost]
        public ActionResult ClearAndClose()
        {
            StopRead();
            ClearMemory();
            return StatusCode(200, "OK");
        }

        #endregion

        #region Internal classes and methods
        private static string szGetInterfaceType(int iDevicetype)
        {
            return "LibUSB";
        }

        class stFoundDevice
        {
            public int iDeviceType;
            public string szDeviceName;
            public string szSerial;
            public string szPath;
            public override string ToString()
            {
                return "Device type: " + szGetInterfaceType(iDeviceType) + ", name: " + szDeviceName + ", serial: " + szSerial + ", path: " + szPath;
            }
            public stFoundDevice()
            {
                iDeviceType = 0;
                szDeviceName = "";
                szSerial = "";
                szPath = "";
            }
        };
        #endregion

        #region enums
        enum DetectorState
        {
            Idle,
            Closed,
            Reading
        }
        #endregion
    }
}
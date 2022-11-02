using System;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using BVA200.Detectors;
using BVA200.Models;
using static BVA200.Logic.Common;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BVA200.Controllers
{
    public class TopazDetectorController : Controller
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
            try
            {
                stFoundDevice oDevice = GetDevice();
                if (oDevice == null)
                {
                    CloseDevice();
                    oDevice = GetDevice();
                    if (oDevice == null)
                    {
                        return StatusCode(503, "No Devices Found");
                    }
                }

                OpenDevice(oDevice);
                HttpContext.Session.SetString("DetectorState", DetectorState.Idle.ToString());
                return StatusCode(200, "Devices: " + oDevice + "   OpenedState: Successful");
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        private stFoundDevice GetDevice()
        {
            int numberOfDevices = DIMHW.FindDevicesEx(DIMHW.DEF_MCA_INTFC_ALL, 500, 1);
            for (int i = 0; i < numberOfDevices; i++)
            {
                // Get the interface
                if (DIMHW.GetDeviceInfo(i, szDevicePath, DEF_STRING_SIZE, szDeviceName, DEF_STRING_SIZE, szSerial, DEF_STRING_SIZE, ref iDeviceType) == DIMHW.MCA_SUCCESS)
                {
                    // make object
                    stFoundDevice oDevice = new stFoundDevice();
                    oDevice.iDeviceType = iDeviceType;
                    oDevice.szPath = szDevicePath.ToString();
                    oDevice.szDeviceName = szDeviceName.ToString();
                    oDevice.szSerial = szSerial.ToString();

                    return oDevice;
                }
            }
            return null;
        }

        [HttpPost]
        public ActionResult CloseTopazReader()
        {
            try
            {
                CloseDevice();
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
                uint result = 0;
                string status = "";
                int iRet;

                // Start acquisition
                var results = DIMHW.StartAcquisition(hMCA);

                // Check the status of the data acquisition.
                iRet = DIMHW.GetAcquisitionStatus(hMCA, ref result);
                if (iRet == 0)
                {
                    if (result == 1)
                    {
                        status = "Acquiring";
                        HttpContext.Session.SetString("DetectorState", DetectorState.Reading.ToString());
                        return StatusCode(200, status);
                    }
                }

                HttpContext.Session.SetString("DetectorState", DetectorState.Idle.ToString());
                status = "Stopped";
                return StatusCode(503, status);

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
                DIMHW.StopAcquisition(hMCA);
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
                DIMHW.RestoreDefaults(hMCA);
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
                HttpContext.Session.SetString("DetectorState", DetectorState.Reading.ToString());
                var result = DIMHW.ClearAll(hMCA);
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
        private void OpenDevice(Object device)
        {
            // Try to open the selected device, first check if our pointer is free
            if (hMCA != IntPtr.Zero)
            {
                // The detector is still open, close it first
                CloseDevice();
            }

            // Try to open the selected detector            
            stFoundDevice oDevice = (stFoundDevice)device;
            StringBuilder szDevicePath = new StringBuilder(DEF_STRING_SIZE);
            szDevicePath.Insert(0, oDevice.szPath);
            int iDeviceType = oDevice.iDeviceType;

            // Open the MCA
            int iRet = DIMHW.OpenDevice(szDevicePath, iDeviceType, ref hMCA);
            if (iRet != DIMHW.MCA_SUCCESS)
            {
                CloseDevice();
                throw new Exception("Failed to open device");
            }
        }
        private void CloseDevice()
        {
            // Close the detector
            DIMHW.CloseDevice(hMCA);

            // zero the MCA pointer
            hMCA = IntPtr.Zero;
        }

        [HttpPost]
        public ActionResult ReadSpectrum()
        {
            if (requestInProcess == true)
            {
                return StatusCode(200, lastRead);
            }
            requestInProcess = true;
            try
            {
                SpectrumRead spectrumRead = new SpectrumRead();

                int iGainFine = 2048, iGainCoarse = 1, iGainDigital = 0, iPHAL = 308, iPHAH = 420, iCurrCount = 0, iHVstatus = 0, iRet = 0;
                float sElapsedRT = 0, sElapsedLT = 0, sHVvalue = 1000;


                // Get spectrum
                if (DIMHW.ReadSpectrum(hMCA, m_aiSpectrum, ref m_iNrOfChannels, ref sElapsedRT, ref sElapsedLT) != DIMHW.MCA_SUCCESS)
                {
                    spectrumRead.Result = "Failed to read spectrum";
                }

                // GET PARAMETERS
                iRet = DIMHW.GetParam(hMCA, DIMHW.DEF_MCA_PARAM_PHA_PRESET_LLD, ref iPHAL, 4);
                iRet = DIMHW.GetParam(hMCA, DIMHW.DEF_MCA_PARAM_PHA_PRESET_ULD, ref iPHAH, 4);
                iRet = DIMHW.GetParam(hMCA, DIMHW.DEF_MCA_PARAM_GAIN_FINE, ref iGainFine, 4);
                iRet = DIMHW.GetParam(hMCA, DIMHW.DEF_MCA_PARAM_GAIN_COARSE, ref iGainCoarse, 4);
                iRet = DIMHW.GetParam(hMCA, DIMHW.DEF_MCA_PARAM_GAIN_DIGITAL, ref iGainDigital, 4);
                iRet = DIMHW.GetParam(hMCA, DIMHW.DEF_MCA_PARAM_CURRENT_COUNTS, ref iCurrCount, 4);

                // High voltage value is a float value, High voltage status 0 = off, 1 = on
                iRet = DIMHW.GetParam(hMCA, DIMHW.DEF_MCA_PARAM_HV_VALUE, ref sHVvalue, 4);
                iRet = DIMHW.GetParam(hMCA, DIMHW.DEF_MCA_PARAM_HV_STATUS, ref iHVstatus, 4);

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

                UInt32[] spectrum = new UInt32[900];
                Array.Copy(m_aiSpectrum, spectrum, 900);

                spectrumRead.DetectorSpectrum = spectrum.ToList();
                spectrumRead.Peaks = new CanberraPeakFinder().FindPeaks(spectrum);
                // This calls SpectrumRead.cs and applys the number to a switch case on how to identify the high and low.
                spectrumRead.Calculate(1);//this tries to figure out what the sample being read is along with meta
                lastRead = spectrumRead;
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
                // Save settings to EEPROM...Note: Avoid calling this unnecessarily, as EEPROM devices have a limited number or erase/write cycles. The memory chips used in
                // bMCA and Topaz devices have a guaranteed life of more than 1000,000 write cycles. Ref pg44 of SDK.
                //iRet = SaveSettings(hDevice);
                //if (Ret == MCA_SUCCESS)
                //MessageBox(NULL, TEXT("Settings saved successfully into EEPROM."), TEXT("bMCA"),
                //MB_OK);
                // iGainCoarse 1 is what is sent when demo proj sends x2, iGainDigital = 5 when demo sends x32, iGainFine = 2214 when demo = 1.554
                int iGainFine = settings.FineGain, iGainCoarse = settings.CoarseGain, iAcqMode = settings.AcqMode, iHVstatus = 0, iAcqPreset = 0, iAcqPresetCounts = 0;
                float iRet = 0;
                // SET DEFAULT PARAMETERS
                // Fine Gain default start point is 2280
                iRet = DIMHW.SetParam(hMCA, DIMHW.DEF_MCA_PARAM_GAIN_FINE, ref iGainFine, 4);
                // Coarse Gain default is 1
                iRet = DIMHW.SetParam(hMCA, DIMHW.DEF_MCA_PARAM_GAIN_COARSE, ref iGainCoarse, 4);
                // Set Aquisition preset type
                iRet = DIMHW.SetParam(hMCA, DIMHW.DEF_MCA_PARAM_ACQ_MODE_EX, ref iAcqMode, 4);
                // Set Aquisition preset time to 0
                iRet = DIMHW.SetParam(hMCA, DIMHW.DEF_MCA_PARAM_ACQ_PRESET, ref iAcqPreset, 4);
                // Set Aquisition preset counts to 0
                iRet = DIMHW.SetParam(hMCA, DIMHW.DEF_MCA_PARAM_PRESET_COUNTS, ref iAcqPresetCounts, 4);
                // Turn high voltage off
                iRet = DIMHW.SetParam(hMCA, DIMHW.DEF_MCA_PARAM_HV_STATUS, ref iHVstatus, 4);
                // iRet = DIMHW.SaveSettings(hMCA);
                return StatusCode(200, 0);
            }
            catch (Exception e)
            {
                return StatusCode(500, e);
            }
        }

        [HttpPost]
        public ActionResult SetDetectorFineGain([FromBody] DetectorSettings settings)
        {
            try
            {
                int iGainFine = settings.FineGain;
                float iRet = 0;
                iRet = DIMHW.SetParam(hMCA, DIMHW.DEF_MCA_PARAM_GAIN_FINE, ref iGainFine, 4);
                if(iRet == 0){
                    return StatusCode(200, 0);
                }else{
                    return StatusCode(500, iRet);
                }
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
                using (var conn = new SQLiteConnection(GetConnectionString()))
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
            CloseDevice();
            return StatusCode(200, "OK");
        }

        #endregion

        #region Internal classes and methods
        private static string szGetInterfaceType(int iDevicetype)
        {
            switch (iDevicetype)
            {
                case DIMHW.DEF_MCA_INTFC_ALL:
                    return "All interfaces";
                case DIMHW.DEF_MCA_INTFC_ETHERNET:
                    return "Ethernet";
                case DIMHW.DEF_MCA_INTFC_LIBUSB:
                    return "LibUSB";
            }
            return "Unknown";
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
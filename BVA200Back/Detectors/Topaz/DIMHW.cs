using System;
using System.Text;
using static BVA200.Logic.Common;

namespace BVA200.Detectors
{
    class DIMHW
    {
        /****************** DECLARATIONS *********************/
        public const Int32 DEF_MCA_INTFC_ALL = 0;
        public const Int32 DEF_MCA_INTFC_LIBUSB = 2;
        public const Int32 DEF_MCA_INTFC_ETHERNET = 3;
        public const Int32 DEF_MCA_INTFC_SIM = 999;

        public const String DEF_MCA_PREFIX_USB = "USB";
        public const String DEF_MCA_PREFIX_ETH = "ETH";
        public const String DEF_MCA_PREFIX_SIM = "SIM";

        public const Int32 DEF_MCA_STRING_LENGTH = 256;

        // Command definitions
        public const Int32 DEF_MCA_COMMAND_GETPARAM = 1;
        public const Int32 DEF_MCA_COMMAND_SETPARAM = 2;
        public const Int32 DEF_MCA_COMMAND_STARTACQ = 3;
        public const Int32 DEF_MCA_COMMAND_STOPACQ = 4;
        public const Int32 DEF_MCA_COMMAND_GETACQSTATUS = 5;
        public const Int32 DEF_MCA_COMMAND_CLEARMEM = 6;
        public const Int32 DEF_MCA_COMMAND_CLEARTIME = 7;
        public const Int32 DEF_MCA_COMMAND_CLEARALL = 8;
        public const Int32 DEF_MCA_COMMAND_SETTESTMODE = 9;
        public const Int32 DEF_MCA_COMMAND_ARMSCOPE = 10;
        public const Int32 DEF_MCA_COMMAND_GETSCOPESTATUS = 11;
        public const Int32 DEF_MCA_COMMAND_READSPECTRUM = 12;
        public const Int32 DEF_MCA_COMMAND_READSCOPE = 13;
        public const Int32 DEF_MCA_COMMAND_SAVESETTINGS = 14;
        public const Int32 DEF_MCA_COMMAND_RESTOREDEFAULTS = 15;
        public const Int32 DEF_MCA_COMMAND_READLISTEVENTS = 16;
        public const Int32 DEF_MCA_COMMAND_SAVEDEFAULTS = 17;

        // Parameter definitions
        public const Int32 DEF_MCA_PARAM_DEV_NAME = 0;	// String
        public const Int32 DEF_MCA_PARAM_SERIAL_NUMBER = 1;	// String
        public const Int32 DEF_MCA_PARAM_FW_VERSION = 2;	// String
        public const Int32 DEF_MCA_PARAM_FW_DATE = 3;	// String
        public const Int32 DEF_MCA_PARAM_PROD_DATE = 4;	// String
        public const Int32 DEF_MCA_PARAM_MCA_MODE = 5;	// Int32
        public const Int32 DEF_MCA_PARAM_NUM_CHANNELS = 6;	// Int32
        public const Int32 DEF_MCA_PARAM_PHA_LLD = 7;	// Int32
        public const Int32 DEF_MCA_PARAM_PHA_ULD = 8;	// Int32
        public const Int32 DEF_MCA_PARAM_ACQ_PRESET = 9;	// Int32 (0.1second) Float
        public const Int32 DEF_MCA_PARAM_ACQ_MODE = 10;	// Int32
        public const Int32 DEF_MCA_PARAM_INP_POLARITY = 11;	// Int32
        public const Int32 DEF_MCA_PARAM_GAIN_DIGITAL = 12;	// Int32
        public const Int32 DEF_MCA_PARAM_GAIN_COARSE = 13;	// Int32
        public const Int32 DEF_MCA_PARAM_GAIN_FINE = 14;	// Int32
        public const Int32 DEF_MCA_PARAM_RISE_TIME = 15;	// Int32
        public const Int32 DEF_MCA_PARAM_FLAT_TOP = 16;	// Int32
        public const Int32 DEF_MCA_PARAM_PZ_ADJ = 17; 	// Int32
        public const Int32 DEF_MCA_PARAM_THRESHOLD = 18;	// Int32
        public const Int32 DEF_MCA_PARAM_BLR_ENABLE = 19;	// Int32
        public const Int32 DEF_MCA_PARAM_PUR_ENABLE = 20;	// Int32
        public const Int32 DEF_MCA_PARAM_HV_VALUE = 21;	// Int32
        public const Int32 DEF_MCA_PARAM_HV_STATUS = 22;	// Int32
        public const Int32 DEF_MCA_PARAM_SCOPE_TRIG_LVL = 23;	// Int32
        public const Int32 DEF_MCA_PARAM_SCOPE_WAVEFORM = 24;	// Int32
        public const Int32 DEF_MCA_PARAM_CALIBRATION = 25;  // Float[]
        public const Int32 DEF_MCA_PARAM_MCS_CHANNELS = 26;  // Int32
        public const Int32 DEF_MCA_PARAM_MCS_CURRENT_CHANNEL = 27;  // Int32
        public const Int32 DEF_MCA_PARAM_PRESET_COUNTS = 28;  // Int32
        public const Int32 DEF_MCA_PARAM_CURRENT_COUNTS = 29;  // Int32
        public const Int32 DEF_MCA_PARAM_ACQ_MODE_EX = 30;  // Int32
        public const Int32 DEF_MCA_PARAM_PHA_PRESET_LLD = 31;	// Int32
        public const Int32 DEF_MCA_PARAM_PHA_PRESET_ULD = 32;	// Int32
        public const Int32 DEF_MCA_PARAM_EXT_COUNTS = 33;	// Int32
        public const Int32 DEF_MCA_PARAM_EXT_IO_MODE = 34;	// Int32
        public const Int32 DEF_MCA_PARAM_ICR_COUNTS = 35;	// Int32
        public const Int32 DEF_MCA_PARAM_HW_PZ_ADJ = 36;	// Int32
        public const Int32 DEF_MCA_PARAM_TRP_COUNTS = 37;	// Int32
        public const Int32 DEF_MCA_PARAM_OCR_COUNTS = 38;	// Int32
        public const Int32 DEF_MCA_PARAM_PUR_GUARD = 39;	// Int32
        public const Int32 DEF_MCA_PARAM_TRP_GUARD = 40;	// Int32
        public const Int32 DEF_MCA_PARAM_LT_TRIM = 41;	// Int32
        public const Int32 DEF_MCA_PARAM_ADC_SAMPLING_RATE = 42;	// Int32
        public const Int32 DEF_MCA_PARAM_TIMING_INFO = 43;  // UInt32[]
        public const Int32 DEF_MCA_PARAM_HV_INFO = 44;  // UInt32[]
        public const Int32 DEF_MCA_PARAM_LIST_MODE = 45;	// Int32
        public const Int32 DEF_MCA_PARAM_SCOPE_TRIG_SRC = 46;  // Int32
        public const Int32 DEF_MCA_PARAM_DIFF_SEL = 47;  // Int32
        public const Int32 DEF_MCA_PARAM_GPIO1_MODE = 48;  // Int32
        public const Int32 DEF_MCA_PARAM_GPIO2_MODE = 49;  // Int32


        public const Int32 DEF_MCA_PARAM_NETBIOS_NAME = 100; // String
        public const Int32 DEF_MCA_PARAM_DEFAULT_IP = 101; // UInt32[]
        public const Int32 DEF_MCA_PARAM_REMOTE_IP = 102; // UInt32
        public const Int32 DEF_MCA_PARAM_REMOTE_PORT = 103;	// UInt16

        public const Int32 DEF_MCA_PARAM_USERID = 250; // String
        public const Int32 DEF_MCA_PARAM_GROUPID = 251; // String
        public const Int32 DEF_MCA_PARAM_USERDATA = 252; // Uint32[]

        public const Int32 DEF_MCA_PARAM_BOOTLOADER = 254;  // UInt32

        // Info string indexes
        public const Int32 DEF_MCA_INFO_DEVICE_PATH = 0;
        public const Int32 DEF_MCA_INFO_DEVICE_NAME = 1;
        public const Int32 DEF_MCA_INFO_DEVICE_TYPE = 2;
        public const Int32 DEF_MCA_INFO_SERIAL_NUMBER = 3;
        public const Int32 DEF_MCA_INFO_USER_NAME = 4;
        public const Int32 DEF_MCA_INFO_GROUP_NAME = 5;

        // Acquisition modes
        public const Int32 DEF_MCA_ACQ_MODE_REAL_TIME = 0x00;
        public const Int32 DEF_MCA_ACQ_MODE_LIVE_TIME = 0x01;
        public const Int32 DEF_MCA_ACQ_MODE_TIME = 0x02;
        public const Int32 DEF_MCA_ACQ_MODE_COUNTS = 0x04;
        public const Int32 DEF_MCA_ACQ_MODE_EXT_CONTROL = 0x08;

        // Acquisition status
        public const Int32 DEF_MCA_ACQ_STATUS_STOPPED = 0x00;
        public const Int32 DEF_MCA_ACQ_STATUS_ACQUIRING = 0x01;
        public const Int32 DEF_MCA_ACQ_STATUS_WAITING = 0x02;
        public const Int32 DEF_MCA_ACQ_STATUS_STANDBY = 0x04;

        // GPIO functions
        public const Int32 DEF_MCA_GPIO_NOP = 0;         // No operation
        public const Int32 DEF_MCA_GPIO_EXT_COUNTER_1_INPUT = 1;
        public const Int32 DEF_MCA_GPIO_EXT_COUNTER_2_INPUT = 2;
        public const Int32 DEF_MCA_GPIO_ROI_1_OUTPUT = 3;
        public const Int32 DEF_MCA_GPIO_ROI_2_OUTPUT = 4;
        public const Int32 DEF_MCA_GPIO_ICR_OUTPUT = 5;
        public const Int32 DEF_MCA_GPIO_OCR_OUTPUT = 6;
        public const Int32 DEF_MCA_GPIO_TRP_INPUT = 7;
        public const Int32 DEF_MCA_GPIO_TRP_OUTPUT = 8;
        public const Int32 DEF_MCA_GPIO_HV_INHIBIT_INPUT = 9;
        public const Int32 DEF_MCA_GPIO_HV_STATUS_OUTPUT = 10;
        public const Int32 DEF_MCA_GPIO_ACQ_START_INPUT = 11;
        public const Int32 DEF_MCA_GPIO_ACQ_STOP_INPUT = 12;
        public const Int32 DEF_MCA_GPIO_ACQ_START_STOP_INPUT = 13;
        public const Int32 DEF_MCA_GPIO_ACQ_SUSPEND_INPUT = 14;
        public const Int32 DEF_MCA_GPIO_ACQ_STANDBY_OUTPUT = 15;
        public const Int32 DEF_MCA_GPIO_ACQ_STATUS_OUTPUT = 16;
        public const Int32 DEF_MCA_GPIO_ACQ_START_STOP_OUTPUT = 17;
        public const Int32 DEF_MCA_GPIO_ACQ_WAITING_OUTPUT = 18;
        public const Int32 DEF_MCA_GPIO_MCS_CHANNEL_ADV_INPUT = 19;
        public const Int32 DEF_MCA_GPIO_MCS_CHANNEL_ADV_OUTPUT = 20;
        public const Int32 DEF_MCA_GPIO_MCS_READY_OUTPUT = 21;
        public const Int32 DEF_MCA_GPIO_MCS_SWEEP_ADV_INPUT = 22;
        public const Int32 DEF_MCA_GPIO_MCS_SWEEP_ADV_OUTPUT = 23;
        public const Int32 DEF_MCA_GPIO_COINCIDENCE_INPUT = 24;
        public const Int32 DEF_MCA_GPIO_ANTICOINCIDENCE_INPUT = 25;
        public const Int32 DEF_MCA_GPIO_LT_GATE_OUTPUT = 26;
        public const Int32 DEF_MCA_GPIO_TLIST_SYNC_INPUT = 27;
        public const Int32 DEF_MCA_GPIO_TLIST_SYNC_OUTPUT = 28;
        public const Int32 DEF_MCA_GPIO_PULSE_GEN_1_OUTPUT = 29;
        public const Int32 DEF_MCA_GPIO_PULSE_GEN_2_OUTPUT = 30;
        public const Int32 DEF_MCA_GPIO_EXT_USER_1_INPUT = 31;
        public const Int32 DEF_MCA_GPIO_EXT_USER_2_INPUT = 32;
        public const Int32 DEF_MCA_GPIO_USER_OUTPUT = 33;
        public const Int32 DEF_MCA_GPIO_MCS_COUNTER_INPUT = 34;
        public const Int32 DEF_MCA_GPIO_PHA_GROUP_SELECT_INPUT = 35;
        public const Int32 DEF_MCA_GPIO_PHA_GROUP_SELECT_OUTPUT = 36;

        public const Int32 DEF_MCA_GPIO_POLARITY_POSITIVE = 0x0000;
        public const Int32 DEF_MCA_GPIO_POLARITY_NEGATIVE = 0x8000;

        // Error codes
        public const Int32 MCA_SUCCESS = 0;
        public const Int32 MCA_ERROR_NOT_CONNECTED = -1;
        public const Int32 MCA_ERROR_ALREADY_OPEN = -2;
        public const Int32 MCA_ERROR_INVALID_SOCKET = -3;
        public const Int32 MCA_ERROR_BIND_FAILED = -4;
        public const Int32 MCA_ERROR_INVALID_PARAMETER = -5;
        public const Int32 MCA_ERROR_INVALID_ARGUMENT = -6;
        public const Int32 MCA_ERROR_NOT_SUPPORTED = -7;
        public const Int32 MCA_ERROR_CONNECTION_FAILED = -8;
        public const Int32 MCA_ERROR_CONNECTION_CLOSED = -9;
        public const Int32 MCA_ERROR_INVALID_RESPONSE = -10;
        public const Int32 MCA_ERROR_SEND_FAILED = -11;
        public const Int32 MCA_ERROR_RECEIVE_FAILED = -12;
        public const Int32 MCA_ERROR_MCA_FAILED = -13;
        public const Int32 MCA_ERROR_INVALID_DEVICE = -14;
        public const Int32 MCA_ERROR_INVALID_HANDLE = -15;
        public const Int32 MCA_ERROR_OUT_OF_RANGE = -16;



        // Open/Close
        public static Int32 FindDevices(Int32 DeviceType)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.FindDevices(DeviceType);
            }
            else
            {
                return DIMHW_Windows.FindDevices(DeviceType);
            }
        }
        public static Int32 FindDevicesEx(Int32 DeviceType, UInt32 Timeout, UInt32 NewSearch)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.FindDevicesEx(DeviceType, Timeout, NewSearch);
            }
            else
            {
                return DIMHW_Windows.FindDevicesEx(DeviceType, Timeout, NewSearch);
            }
        }
        public static Int32 GetDeviceInfo(Int32 InterfaceNr, StringBuilder DevicePath, Int32 DevicePathSize, StringBuilder DeviceName, Int32 DeviceNameSize, StringBuilder Serial, Int32 SerialSize, ref Int32 DeviceType)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.GetDeviceInfo(InterfaceNr, DevicePath, DevicePathSize, DeviceName, DeviceNameSize, Serial, SerialSize, ref DeviceType);
            }
            else
            {
                return DIMHW_Windows.GetDeviceInfo(InterfaceNr, DevicePath, DevicePathSize, DeviceName, DeviceNameSize, Serial, SerialSize, ref DeviceType);
            }
        }
        public static Int32 GetDeviceInfoEx(Int32 InterfaceNr, StringBuilder Info, UInt32 InfoSize, UInt32 InfoIndex)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.GetDeviceInfoEx(InterfaceNr, Info, InfoSize, InfoIndex);
            }
            else
            {
                return DIMHW_Windows.GetDeviceInfoEx(InterfaceNr, Info, InfoSize, InfoIndex);
            }
        }
        public static Int32 OpenDevice(StringBuilder DevicePath, Int32 DeviceType, ref IntPtr DeviceHandle)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.OpenDevice(DevicePath, DeviceType, ref DeviceHandle);
            }
            else
            {
                return DIMHW_Windows.OpenDevice(DevicePath, DeviceType, ref DeviceHandle);
            }
        }
        public static Int32 OpenDeviceEx(StringBuilder DevicePath, Int32 DeviceType, ref IntPtr DeviceHandle, UInt32 Timeout)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.OpenDeviceEx(DevicePath, DeviceType, ref DeviceHandle, Timeout);
            }
            else
            {
                return DIMHW_Windows.OpenDeviceEx(DevicePath, DeviceType, ref DeviceHandle, Timeout);
            }
        }
        public static Int32 CloseDevice(IntPtr DeviceHandle)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.CloseDevice(DeviceHandle);
            }
            else
            {
                return DIMHW_Windows.CloseDevice(DeviceHandle);
            }
        }

        // Get parameter
        public static Int32 GetParam(IntPtr DeviceHandle, Int32 ParamId, StringBuilder ParamValue, Int32 ParamSize = 0)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.GetParam(DeviceHandle, ParamId, ParamValue, ParamSize = 0);
            }
            else
            {
                return DIMHW_Windows.GetParam(DeviceHandle, ParamId, ParamValue, ParamSize = 0);
            }
        }
        public static Int32 GetParam(IntPtr DeviceHandle, Int32 ParamId, Int32[] ParamValue, Int32 ParamSize = 0)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.GetParam(DeviceHandle, ParamId, ParamValue, ParamSize = 0);
            }
            else
            {
                return DIMHW_Windows.GetParam(DeviceHandle, ParamId, ParamValue, ParamSize = 0);
            }
        }
        public static Int32 GetParam(IntPtr DeviceHandle, Int32 ParamId, ref float[] ParamValue, Int32 ParamSize = 0)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.GetParam(DeviceHandle, ParamId, ref ParamValue, ParamSize = 0);
            }
            else
            {
                return DIMHW_Windows.GetParam(DeviceHandle, ParamId, ref ParamValue, ParamSize = 0);
            }
        }
        public static Int32 GetParam(IntPtr DeviceHandle, Int32 ParamId, ref Int32 ParamValue, Int32 ParamSize = 4)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.GetParam(DeviceHandle, ParamId, ref ParamValue, ParamSize = 4);
            }
            else
            {
                return DIMHW_Windows.GetParam(DeviceHandle, ParamId, ref ParamValue, ParamSize = 4);
            }
        }
        public static Int32 GetParam(IntPtr DeviceHandle, Int32 ParamId, ref float ParamValue, Int32 ParamSize = 4)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.GetParam(DeviceHandle, ParamId, ref ParamValue, ParamSize = 4);
            }
            else
            {
                return DIMHW_Windows.GetParam(DeviceHandle, ParamId, ref ParamValue, ParamSize = 4);
            }
        }
        // Set parameter
        public static Int32 SetParam(IntPtr DeviceHandle, Int32 ParamId, StringBuilder ParamValue, Int32 ParamSize = 0)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.SetParam(DeviceHandle, ParamId, ParamValue, ParamSize = 0);
            }
            else
            {
                return DIMHW_Windows.SetParam(DeviceHandle, ParamId, ParamValue, ParamSize = 0);
            }
        }
        public static Int32 SetParam(IntPtr DeviceHandle, Int32 ParamId, Int32[] ParamValue, Int32 ParamSize = 0)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.SetParam(DeviceHandle, ParamId, ParamValue, ParamSize = 0);
            }
            else
            {
                return DIMHW_Windows.SetParam(DeviceHandle, ParamId, ParamValue, ParamSize = 0);
            }
        }
        public static Int32 SetParam(IntPtr DeviceHandle, Int32 ParamId, ref float[] ParamValue, Int32 ParamSize = 0)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.SetParam(DeviceHandle, ParamId, ref ParamValue, ParamSize = 0);
            }
            else
            {
                return DIMHW_Windows.SetParam(DeviceHandle, ParamId, ref ParamValue, ParamSize = 0);
            }
        }
        public static Int32 SetParam(IntPtr DeviceHandle, Int32 ParamId, ref Int32 ParamValue, Int32 ParamSize = 4)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.SetParam(DeviceHandle, ParamId, ref ParamValue, ParamSize = 4);
            }
            else
            {
                return DIMHW_Windows.SetParam(DeviceHandle, ParamId, ref ParamValue, ParamSize = 4);
            }
        }
        public static Int32 SetParam(IntPtr DeviceHandle, Int32 ParamId, ref float ParamValue, Int32 ParamSize = 4)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.SetParam(DeviceHandle, ParamId, ref ParamValue, ParamSize = 4);
            }
            else
            {
                return DIMHW_Windows.SetParam(DeviceHandle, ParamId, ref ParamValue, ParamSize = 4);
            }
        }
        // Start acquisition
        public static Int32 StartAcquisition(IntPtr DeviceHandle)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.StartAcquisition(DeviceHandle);
            }
            else
            {
                return DIMHW_Windows.StartAcquisition(DeviceHandle);
            }
        }
        // Stop acquisition
        public static Int32 StopAcquisition(IntPtr DeviceHandle)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.StopAcquisition(DeviceHandle);
            }
            else
            {
                return DIMHW_Windows.StopAcquisition(DeviceHandle);
            }
        }
        // Get status
        public static Int32 GetAcquisitionStatus(IntPtr DeviceHandle, ref UInt32 iAcquisitionStatus)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.GetAcquisitionStatus(DeviceHandle, ref iAcquisitionStatus);
            }
            else
            {
                return DIMHW_Windows.GetAcquisitionStatus(DeviceHandle, ref iAcquisitionStatus);
            }
        }
        // Clear memory
        public static Int32 ClearMemory(IntPtr DeviceHandle)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.ClearMemory(DeviceHandle);
            }
            else
            {
                return DIMHW_Windows.ClearMemory(DeviceHandle);
            }
        }
        // Clear acq time
        public static Int32 ClearTime(IntPtr DeviceHandle)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.ClearTime(DeviceHandle);
            }
            else
            {
                return DIMHW_Windows.ClearTime(DeviceHandle);
            }
        }
        // Clear everything
        public static Int32 ClearAll(IntPtr DeviceHandle)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.ClearAll(DeviceHandle);
            }
            else
            {
                return DIMHW_Windows.ClearAll(DeviceHandle);
            }
        }
        // Internal testing
        public static Int32 SetTestMode(IntPtr DeviceHandle, Int32 OnOff, Int32 Rate)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.SetTestMode(DeviceHandle, OnOff, Rate);
            }
            else
            {
                return DIMHW_Windows.SetTestMode(DeviceHandle, OnOff, Rate);
            }
        }
        // Arm the scope
        public static Int32 ArmScope(IntPtr DeviceHandle)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.ArmScope(DeviceHandle);
            }
            else
            {
                return DIMHW_Windows.ArmScope(DeviceHandle);
            }
        }
        // Get scope status
        public static Int32 GetScopeStatus(IntPtr DeviceHandle, ref UInt32 ScopeStatus)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.GetScopeStatus(DeviceHandle, ref ScopeStatus);
            }
            else
            {
                return DIMHW_Windows.GetScopeStatus(DeviceHandle, ref ScopeStatus);
            }
        }
        // Get the spectrum
        public static Int32 ReadSpectrum(IntPtr DeviceHandle, UInt32[] Spectrum, ref UInt32 NrOfChannels, ref float ElapsedRT, ref float ElapsedLT)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.ReadSpectrum(DeviceHandle, Spectrum, ref NrOfChannels, ref ElapsedRT, ref ElapsedLT);
            }
            else
            {
                return DIMHW_Windows.ReadSpectrum(DeviceHandle, Spectrum, ref NrOfChannels, ref ElapsedRT, ref ElapsedLT);
            }
        }
        // Get the LIST/TLIST mode events
        public static Int32 ReadListEvents(IntPtr DeviceHandle, UInt32[] List, UInt32 MaxEvents, ref UInt32 NumEvents, ref UInt32 Overflow)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.ReadListEvents(DeviceHandle, List, MaxEvents, ref NumEvents, ref Overflow);
            }
            else
            {
                return DIMHW_Windows.ReadListEvents(DeviceHandle, List, MaxEvents, ref NumEvents, ref Overflow);
            }
        }
        // Get scope waveform
        public static Int32 ReadScope(IntPtr DeviceHandle, Int16[] Waveform)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.ReadScope(DeviceHandle, Waveform);
            }
            else
            {
                return DIMHW_Windows.ReadScope(DeviceHandle, Waveform);
            }
        }
        public static Int32 ReadScopeEx(IntPtr DeviceHandle, Int16[] Ch1Waveform, Int16[] Ch2Waveform, Int16[] DigitalBits)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.ReadScopeEx(DeviceHandle, Ch1Waveform, Ch2Waveform, DigitalBits);
            }
            else
            {
                return DIMHW_Windows.ReadScopeEx(DeviceHandle, Ch1Waveform, Ch2Waveform, DigitalBits);
            }
        }
        // Save settings into device's EEPROM
        public static Int32 SaveSettings(IntPtr DeviceHandle)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.SaveSettings(DeviceHandle);
            }
            else
            {
                return DIMHW_Windows.SaveSettings(DeviceHandle);
            }
        }
        // Restore default factory settings
        public static Int32 RestoreDefaults(IntPtr DeviceHandle)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.RestoreDefaults(DeviceHandle);
            }
            else
            {
                return DIMHW_Windows.RestoreDefaults(DeviceHandle);
            }
        }
        // Direct device access
        public static Int32 DirectIO(IntPtr DeviceHandle, byte[] Buffer, Int32 Length, Int32 Mode)
        {
            if (IsLinux())
            {
                return DIMHW_Linux.DirectIO(DeviceHandle, Buffer, Length, Mode);
            }
            else
            {
                return DIMHW_Windows.DirectIO(DeviceHandle, Buffer, Length, Mode);
            }
        }
    }
}

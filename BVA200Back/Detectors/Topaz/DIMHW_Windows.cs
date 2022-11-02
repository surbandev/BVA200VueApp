using System;
using System.Text;
using System.Runtime.InteropServices;

namespace BVA200.Detectors
{
    class DIMHW_Windows
    {
        public const string libraryString = ".\\Detectors\\Topaz\\DIMHW.dll";
        
        // Open/Close
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "FindDevices", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 FindDevices(Int32 DeviceType);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "FindDevicesEx", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 FindDevicesEx(Int32 DeviceType, UInt32 Timeout, UInt32 NewSearch);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "GetDeviceInfo", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 GetDeviceInfo(Int32 InterfaceNr, StringBuilder DevicePath, Int32 DevicePathSize, StringBuilder DeviceName, Int32 DeviceNameSize, StringBuilder Serial, Int32 SerialSize, ref Int32 DeviceType);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "GetDeviceInfoEx", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 GetDeviceInfoEx(Int32 InterfaceNr, StringBuilder Info, UInt32 InfoSize, UInt32 InfoIndex);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "OpenDevice", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 OpenDevice(StringBuilder DevicePath, Int32 DeviceType, ref IntPtr DeviceHandle);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "OpenDeviceEx", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 OpenDeviceEx(StringBuilder DevicePath, Int32 DeviceType, ref IntPtr DeviceHandle, UInt32 Timeout);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "CloseDevice", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 CloseDevice(IntPtr DeviceHandle);

        // Get parameter
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "GetParam", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 GetParam(IntPtr DeviceHandle, Int32 ParamId, StringBuilder ParamValue, Int32 ParamSize = 0);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "GetParam", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 GetParam(IntPtr DeviceHandle, Int32 ParamId, Int32[] ParamValue, Int32 ParamSize = 0);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "GetParam", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 GetParam(IntPtr DeviceHandle, Int32 ParamId, ref float[] ParamValue, Int32 ParamSize = 0);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "GetParam", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 GetParam(IntPtr DeviceHandle, Int32 ParamId, ref Int32 ParamValue, Int32 ParamSize = 4);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "GetParam", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 GetParam(IntPtr DeviceHandle, Int32 ParamId, ref float ParamValue, Int32 ParamSize = 4);
        // Set parameter
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "SetParam", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 SetParam(IntPtr DeviceHandle, Int32 ParamId, StringBuilder ParamValue, Int32 ParamSize = 0);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "SetParam", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 SetParam(IntPtr DeviceHandle, Int32 ParamId, Int32[] ParamValue, Int32 ParamSize = 0);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "SetParam", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 SetParam(IntPtr DeviceHandle, Int32 ParamId, ref float[] ParamValue, Int32 ParamSize = 0);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "SetParam", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 SetParam(IntPtr DeviceHandle, Int32 ParamId, ref Int32 ParamValue, Int32 ParamSize = 4);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "SetParam", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 SetParam(IntPtr DeviceHandle, Int32 ParamId, ref float ParamValue, Int32 ParamSize = 4);
        // Start acquisition
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "StartAcquisition", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 StartAcquisition(IntPtr DeviceHandle);
        // Stop acquisition
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "StopAcquisition", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 StopAcquisition(IntPtr DeviceHandle);
        // Get status
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "GetAcquisitionStatus", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 GetAcquisitionStatus(IntPtr DeviceHandle, ref UInt32 iAcquisitionStatus);
        // Clear memory
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "ClearMemory", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 ClearMemory(IntPtr DeviceHandle);
        // Clear acq time
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "ClearTime", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 ClearTime(IntPtr DeviceHandle);
        // Clear everything
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "ClearAll", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 ClearAll(IntPtr DeviceHandle);
        // Internal testing
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "SetTestMode", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 SetTestMode(IntPtr DeviceHandle, Int32 OnOff, Int32 Rate);
        // Arm the scope
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "ArmScope", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 ArmScope(IntPtr DeviceHandle);
        // Get scope status
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "GetScopeStatus", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 GetScopeStatus(IntPtr DeviceHandle, ref UInt32 ScopeStatus);
        // Get the spectrum
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "ReadSpectrum", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 ReadSpectrum(IntPtr DeviceHandle, UInt32[] Spectrum, ref UInt32 NrOfChannels, ref float ElapsedRT, ref float ElapsedLT);
        // Get the LIST/TLIST mode events
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "ReadListEvents", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 ReadListEvents(IntPtr DeviceHandle, UInt32[] List, UInt32 MaxEvents, ref UInt32 NumEvents, ref UInt32 Overflow);
        // Get scope waveform
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "ReadScope", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 ReadScope(IntPtr DeviceHandle, Int16[] Waveform);
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "ReadScopeEx", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 ReadScopeEx(IntPtr DeviceHandle, Int16[] Ch1Waveform, Int16[] Ch2Waveform, Int16[] DigitalBits);
        // Save settings into device's EEPROM
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "SaveSettings", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 SaveSettings(IntPtr DeviceHandle);
        // Restore default factory settings
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "RestoreDefaults", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 RestoreDefaults(IntPtr DeviceHandle);
        // Direct device access
        [DllImport(libraryString, CharSet = CharSet.Ansi, EntryPoint = "DirectIO", CallingConvention = CallingConvention.StdCall)]
        public static extern Int32 DirectIO(IntPtr DeviceHandle, byte[] Buffer, Int32 Length, Int32 Mode);
    }
}

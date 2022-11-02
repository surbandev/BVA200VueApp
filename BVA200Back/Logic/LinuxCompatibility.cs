using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace BVA200.Logic
{
    public static class LinuxCompatibility
    {
        #region LinuxCompatibility
        private const int RTLD_LAZY = 0x00001; //Only resolve symbols as needed
        private const int RTLD_GLOBAL = 0x00100; //Make symbols available to libraries loaded later
        [DllImport("libdl.so.2")]
        //[DllImport("dl")]
        private static extern IntPtr dlopen(string file, int mode);
        #endregion



        public static string RunLinuxCommand(String cmd, int timeout=2500)
        {
            Process process = new Process();
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = "-c \" " + cmd + " \"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit(timeout);

            if (string.IsNullOrEmpty(error)) { 
                return output; 
            }
            else { 
                return error;
            }
        }

        public static void PreLoadLinuxDrivers()
        {
            dlopen("libusb-1.0.so.0", RTLD_LAZY | RTLD_GLOBAL);
        }

        public static void TTS(String StringToSpeak)
        {
            //http://espeak.sourceforge.net/languages.html
            var languages = new String[] { "es", "fr", "de", "en-us" };
            String selectedLanguage = "en-us";
            //option 1
            RunLinuxCommand("espeak -v " + selectedLanguage + " '" + StringToSpeak + "'");
        }

        public static string[] GetAvailablePrinters(){
            string output = RunLinuxCommand("lpstat -p");
            string[] tPrinters = output.Split("\n");
            List<string> printers = new List<string>();
            foreach(var printer in tPrinters){
                string name = printer.Replace("printer ","");
                name = name.Split(" ")[0];
                if (!String.IsNullOrEmpty(name)){
                    printers.Add(name);
                }
            }
            return printers.ToArray<string>();
        }

        public static void PrintFile(string path, string printer){
            string cmd = "lp -d \""+printer+"\" "+path;
            RunLinuxCommand(cmd);
        }
    }
}
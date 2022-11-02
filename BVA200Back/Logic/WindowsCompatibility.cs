using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace BVA200.Logic
{
    public static class WindowsCompatibility
    {
        public static string[] GetAvailablePrinters()
        {
            string output = RunWindowsCommand("wmic printer get name");
            string[] tPrinters = output.Split("\r\r");
            tPrinters = tPrinters.Skip(1).ToArray();
            List<string> printers = new List<string>();
            foreach (var printer in tPrinters)
            {
                string name = printer.Replace("\n", "").Trim();
                if (!String.IsNullOrEmpty(name))
                {
                    printers.Add(name);
                }
            }
            return printers.ToArray();
        }

        public static void PrintFile(string path, string printer)
        {
            string cmd = "Get-Content -Path " + path + " | Out-Printer '" + printer + "'";
            RunWindowsCommand(cmd);
        }

        public static string RunWindowsCommand(String cmd, int timeout = 2500)
        {
            Process process = new Process();
            process.StartInfo.FileName = "PowerShell.exe";
            process.StartInfo.Arguments = "-command " + cmd;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            if (string.IsNullOrEmpty(error))
            {
                return output;
            }
            else
            {
                return error;
            }
        }
    }
}
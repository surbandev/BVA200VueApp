using System;
using System.Data.SQLite;
using System.Linq;
using BVA200.Models;
using Dapper;
using System.Runtime.InteropServices;
using System.Configuration;

namespace BVA200.Logic
{
    public static class Common
    {
        public static string[] GetAvailablePrinters(){
            if (IsLinux()){
                return LinuxCompatibility.GetAvailablePrinters();
            }else{
                return WindowsCompatibility.GetAvailablePrinters();
            }
        }

        public static void PrintFile(string path, string printer){
            if (IsLinux()){
                LinuxCompatibility.PrintFile(path, printer);
            }else{
                WindowsCompatibility.PrintFile(path, printer);
            }
        }
        public static string GetConnectionString(string id = "")
        {
            if (String.IsNullOrEmpty(id))
            {
                if (IsLinux())
                {
                    id = "DaxorLabInternalSQLite_Linux";
                }
                else
                {
                    id = "DaxorLabInternalSQLite";
                }
            }
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
        public static Boolean IsLinux()
        {
            Boolean isLinux = System.Runtime.InteropServices.RuntimeInformation
                                               .IsOSPlatform(OSPlatform.Linux);
            return isLinux;
        }
        public static string GetUTCNowAsISO()
        {
            return DateTime.Now.ToUniversalTime().ToString("o");
        }
        public static DateTime GetNow()
        {
            return DateTime.Now;
        }
        public static Object GetSetting(string setting)
        {
            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                var result = conn.Query<BVAUnitSetting>(@"SELECT Value
                                        FROM Settings
                                        WHERE Setting = @Setting",
                                        new
                                        {
                                            @Setting = setting
                                        }).FirstOrDefault();
                var value = result.Value;
                return value;
            }
        }
    }
}
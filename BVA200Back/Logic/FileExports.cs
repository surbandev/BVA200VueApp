//https://stackoverflow.com/questions/58170376/is-it-possible-to-create-a-spreadsheet-in-memory-and-serve-that-to-a-link-withou
using System;
using System.IO;
using BVA200.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Dapper;
using System.Data.SQLite;
using static BVA200.Logic.LinuxCompatibility;
using static BVA200.Logic.WindowsCompatibility;
using static BVA200.Logic.Common;
using System.Threading;

namespace BVA200.Logic
{
    public static class FileExports
    {
        public static string? GetUSBFlashDrivePath()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                try
                {
                    if (d.VolumeLabel.Contains("efi") || d.VolumeLabel.Contains("boot"))
                    {
                        continue;
                    }
                    if (d.DriveFormat != "msdos" && !d.DriveFormat.Contains("FAT"))
                    {
                        continue;
                    }
                    if (d.IsReady == true)
                    {
                        return d.RootDirectory.FullName;
                    }
                }
                catch
                {
                    //Exception is thrown if dotnet tries to access restricted filesystems.
                }
            }
            return null;
        }

        public static byte[] BuildTestResultsExcel(ExportModel tre)
        {
            using (var conn = new SQLiteConnection(GetConnectionString()))
            {
                int[] RequestedIDs = (int[])tre.RequestedIDs;
                string sql = "SELECT * FROM TestResults";
                if (RequestedIDs.Length > 0 && tre.AllResults == false)//quick null check
                {
                    sql += " WHERE ID IN (";
                    foreach (int id in RequestedIDs)
                    {
                        sql += id + ",";
                    }
                    sql = sql.Trim(',');
                    sql += ");";
                }

                var testResults = conn.Query<TestResult>(sql);

                using (MemoryStream mem = new MemoryStream())
                {
                    SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(mem, SpreadsheetDocumentType.Workbook);

                    // Add a WorkbookPart to the document.
                    WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
                    workbookpart.Workbook = new Workbook();

                    // Add a WorksheetPart to the WorkbookPart.
                    WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                    SheetData sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    // Add Sheets to the Workbook.
                    Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.
                        AppendChild<Sheets>(new Sheets());

                    // Append a new worksheet and associate it with the workbook.
                    Sheet sheet = new Sheet()
                    {
                        Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "BVA Test Results"
                    };

                    //deal with header row
                    Row row = new Row() { RowIndex = 1 };

                    row.Append(new Cell() { CellReference = "A1", CellValue = new CellValue("Test Date"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "B1", CellValue = new CellValue("PatientID"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "C1", CellValue = new CellValue("Weight (kg)"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "D1", CellValue = new CellValue("Height (cm)"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "E1", CellValue = new CellValue("Pregnant?"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "F1", CellValue = new CellValue("Amputee %"), DataType = CellValues.String });//25, 50, 75, 100?
                    row.Append(new Cell() { CellReference = "G1", CellValue = new CellValue("Age"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "H1", CellValue = new CellValue("Whole Blood?"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "I1", CellValue = new CellValue("Injectate Lot #"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "J1", CellValue = new CellValue("IBV"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "K1", CellValue = new CellValue("TBV"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "L1", CellValue = new CellValue("TBV Dev %"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "M1", CellValue = new CellValue("PHCT"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "N1", CellValue = new CellValue("NHCT"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "O1", CellValue = new CellValue("PV"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "P1", CellValue = new CellValue("PV Dev %"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "Q1", CellValue = new CellValue("RBCV"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "R1", CellValue = new CellValue("RBCV Dev %"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "S1", CellValue = new CellValue("BC CT"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "T1", CellValue = new CellValue("PrIJ CT"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "U1", CellValue = new CellValue("STD CT"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "V1", CellValue = new CellValue("PoIJ 1 CT"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "W1", CellValue = new CellValue("PoIJ 2 CT"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "X1", CellValue = new CellValue("PoIJ 3 CT"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "Y1", CellValue = new CellValue("PoIJ 4 CT"), DataType = CellValues.String });
                    row.Append(new Cell() { CellReference = "Z1", CellValue = new CellValue("PoIJ 5 CT"), DataType = CellValues.String });

                    sheetData.Append(row);

                    uint i = 1;
                    foreach (var tr in testResults)
                    {
                        i++;
                        Row dataRow = new Row() { RowIndex = i };
                        string tPregnant = tr.Pregnant == true ? "Yes" : "No";
                        int ampPercent = 0;
                        dataRow.Append(new Cell() { CellReference = "A" + i.ToString(), CellValue = new CellValue(tr.Created ?? DateTime.Now), DataType = CellValues.Date });
                        dataRow.Append(new Cell() { CellReference = "B" + i.ToString(), CellValue = new CellValue(tr.PatientID), DataType = CellValues.String });
                        dataRow.Append(new Cell() { CellReference = "C" + i.ToString(), CellValue = new CellValue(tr.Weight), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "D" + i.ToString(), CellValue = new CellValue(tr.Height), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "E" + i.ToString(), CellValue = new CellValue(tPregnant), DataType = CellValues.String });
                        dataRow.Append(new Cell() { CellReference = "F" + i.ToString(), CellValue = new CellValue(ampPercent), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "G" + i.ToString(), CellValue = new CellValue(tr.Age), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "H" + i.ToString(), CellValue = new CellValue(Convert.ToInt16(tr.WholeBlood)), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "I" + i.ToString(), CellValue = new CellValue(tr.InjectateLotNumber), DataType = CellValues.String });
                        dataRow.Append(new Cell() { CellReference = "J" + i.ToString(), CellValue = new CellValue(tr.IBV), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "K" + i.ToString(), CellValue = new CellValue(tr.TBV), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "L" + i.ToString(), CellValue = new CellValue(tr.TBVDeviation), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "M" + i.ToString(), CellValue = new CellValue(tr.PHCTAVG), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "N" + i.ToString(), CellValue = new CellValue(tr.NHCTAVG), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "O" + i.ToString(), CellValue = new CellValue(tr.PV), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "P" + i.ToString(), CellValue = new CellValue(tr.PVDeviation), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "Q" + i.ToString(), CellValue = new CellValue(tr.RBCV), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "R" + i.ToString(), CellValue = new CellValue(tr.RBCVDeviation), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "S" + i.ToString(), CellValue = new CellValue(tr.BackgroundCount), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "T" + i.ToString(), CellValue = new CellValue(tr.BaselineCount), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "U" + i.ToString(), CellValue = new CellValue(tr.StandardCount), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "V" + i.ToString(), CellValue = new CellValue(Convert.ToInt32(tr.PostInjection1Count)), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "W" + i.ToString(), CellValue = new CellValue(Convert.ToInt32(tr.PostInjection2Count ?? 0)), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "X" + i.ToString(), CellValue = new CellValue(Convert.ToInt32(tr.PostInjection3Count ?? 0)), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "Y" + i.ToString(), CellValue = new CellValue(Convert.ToInt32(tr.PostInjection4Count ?? 0)), DataType = CellValues.Number });
                        dataRow.Append(new Cell() { CellReference = "Z" + i.ToString(), CellValue = new CellValue(Convert.ToInt32(tr.PostInjection5Count ?? 0)), DataType = CellValues.Number });

                        sheetData.Append(dataRow);
                    }

                    sheets.Append(sheet);
                    workbookpart.Workbook.Save();

                    // Close the document.
                    spreadsheetDocument.Close();

                    return mem.ToArray();
                }
            }
        }

        public static byte[] BuildTestResultPDF(SingleTestResult tr)
        {
            string templateHTML = File.ReadAllText("./Resources/testresult_template.html");
            bool amputee = tr.Amputee > 0;

            int ibvHigh = Convert.ToInt32(tr.IBV + (tr.IBV * Convert.ToDecimal(0.1)));
            int ibvLow = Convert.ToInt32(tr.IBV - (tr.IBV * Convert.ToDecimal(0.1)));
            string ibvReferenceRange = ibvLow + " - " + ibvHigh;

            int rbcvHigh = Convert.ToInt32(tr.RBCV + (tr.RBCV * Convert.ToDecimal(0.1)));
            int rbcvLow = Convert.ToInt32(tr.RBCV - (tr.RBCV * Convert.ToDecimal(0.1)));
            string rbcvReferenceRange = rbcvLow + " - " + rbcvHigh;

            int pvHigh = Convert.ToInt32(tr.PV + (tr.PV * Convert.ToDecimal(0.1)));
            int pvLow = Convert.ToInt32(tr.PV - (tr.PV * Convert.ToDecimal(0.1)));
            string pvReferenceRange = pvLow + " - " + pvHigh;

            string tbvDeviationFlag = Math.Abs(tr.TBVDeviation) > 10 ? "*" : "";
            string rbcvDeviationFlag = Math.Abs(tr.RBCVDeviation) > 10 ? "*" : "";
            string pvDeviationFlag = Math.Abs(tr.PVDeviation) > 10 ? "*" : "";

            int hctHigh = 0;
            int hctLow = 0;
            int ihct = 0;

            if (tr.Sex.ToUpper() == "MALE")
            {
                hctLow = 38;
                hctHigh = 49;
                ihct = 44;
            }
            else
            {
                hctLow = 36;
                hctHigh = 45;
                ihct = 40;
            }

            decimal tLowHct = Math.Min(tr.PHCTAVG, ihct);
            decimal tHighHct = Math.Max(tr.PHCTAVG, ihct);
            int hctDeviation = 100 - Convert.ToInt32(Math.Abs(tLowHct / tHighHct) * 100);

            string hctReferenceRange = hctLow + " - " + hctHigh;
            bool hctNeedFlag = tr.PHCTAVG > hctHigh || tr.PHCTAVG < hctLow;
            string hctFlag = hctNeedFlag ? "*" : "";

            int nhctHigh = 0;
            int nhctLow = 0;
            int inhct = 0;

            if (tr.Sex.ToUpper() == "MALE")
            {
                nhctLow = 38;
                nhctHigh = 49;
                inhct = 44;
            }
            else
            {
                nhctLow = 36;
                nhctHigh = 45;
                inhct = 40;
            }

            decimal tLowNhct = Math.Min(tr.NHCTAVG, inhct);
            decimal tHighNhct = Math.Max(tr.NHCTAVG, inhct);
            int nhctDeviation = 100 - Convert.ToInt32(Math.Abs(tLowNhct / tHighNhct) * 100);

            string nhctReferenceRange = nhctLow + " - " + nhctHigh;
            bool nhctNeedFlag = tr.NHCTAVG > nhctHigh || tr.NHCTAVG < nhctLow;
            string nhctFlag = nhctNeedFlag ? "*" : "";

            /*albumin transudation (min = 0.125%, max = 0.375%)
                Normal: 0.0 <= m < 0.25%
                Elevated: 0.25 <= m <= 0.4%
                High: 0.4 <= m <= 0.5%
                Very High: m > 0.5%
            */
            string albuminTransudation = tr.AlbuminTransudationRate.ToString("0.00000");
            string albuminTransudationFlag = tr.AlbuminTransudationRate < 0.0M || tr.AlbuminTransudationRate > 0.25M ? "*" : "";
            string albuminTransudationReferenceRange = "0.0% - 0.25%";
            string albuminTransudationNotes = "";

            decimal weightDeviation = 0.0M;


            //Now replace ALL THE THINGS
            templateHTML = templateHTML.Replace("{{PT_ID}}", tr.PatientID);
            templateHTML = templateHTML.Replace("{{SEX}}", tr.Sex);
            templateHTML = templateHTML.Replace("{{PREGNANT_STATUS}}", Convert.ToString(tr.Pregnant));
            templateHTML = templateHTML.Replace("{{ANALYZED_ON}}", Convert.ToString(tr.Created));
            templateHTML = templateHTML.Replace("{{AMPUTEE_STATUS}}", Convert.ToString(amputee));
            templateHTML = templateHTML.Replace("{{WT}}", Convert.ToString(tr.Weight));
            templateHTML = templateHTML.Replace("{{HT}}", Convert.ToString(tr.Height));
            templateHTML = templateHTML.Replace("{{AGE}}", Convert.ToString(tr.Age));
            templateHTML = templateHTML.Replace("{{WT_DEV}}", Convert.ToString(weightDeviation));

            templateHTML = templateHTML.Replace("{{TBV_DEV}}", Convert.ToString(tr.TBVDeviation));
            templateHTML = templateHTML.Replace("{{TBV_DEV_FG}}", tbvDeviationFlag);
            templateHTML = templateHTML.Replace("{{TBV_DEV_REF_RG}}", "-10% - +10%");
            templateHTML = templateHTML.Replace("{{TBV_DEV_NT}}", Convert.ToString(""));

            templateHTML = templateHTML.Replace("{{RBCV_DEV}}", tr.RBCVDeviation.ToString("0.0"));
            templateHTML = templateHTML.Replace("{{RBCV_DEV_FG}}", rbcvDeviationFlag);
            templateHTML = templateHTML.Replace("{{RBCV_DEV_REF_RG}}", "-10% - +10%");
            templateHTML = templateHTML.Replace("{{RBCV_DEV_NT}}", Convert.ToString(""));

            templateHTML = templateHTML.Replace("{{PV_DEV}}", tr.PVDeviation.ToString("0.0"));
            templateHTML = templateHTML.Replace("{{PV_DEV_FG}}", pvDeviationFlag);
            templateHTML = templateHTML.Replace("{{PV_DEV_REF_RG}}", "-10% - +10%");
            templateHTML = templateHTML.Replace("{{PV_DEV_NT}}", Convert.ToString(""));

            templateHTML = templateHTML.Replace("{{TBV}}", Convert.ToString(tr.TBV));
            templateHTML = templateHTML.Replace("{{TBV_FG}}", tbvDeviationFlag);
            templateHTML = templateHTML.Replace("{{TBV_REF_RG}}", ibvReferenceRange);
            templateHTML = templateHTML.Replace("{{TBV_NT}}", Convert.ToString(""));

            templateHTML = templateHTML.Replace("{{RBCV}}", Convert.ToString(tr.RBCV));
            templateHTML = templateHTML.Replace("{{RBCV_FG}}", rbcvDeviationFlag);
            templateHTML = templateHTML.Replace("{{RBCV_REF_RG}}", rbcvReferenceRange);
            templateHTML = templateHTML.Replace("{{RBCV_NT}}", Convert.ToString(""));

            templateHTML = templateHTML.Replace("{{PV}}", Convert.ToString(tr.PV));
            templateHTML = templateHTML.Replace("{{PV_FG}}", pvDeviationFlag);
            templateHTML = templateHTML.Replace("{{PV_REF_RG}}", pvReferenceRange);
            templateHTML = templateHTML.Replace("{{PV_NT}}", Convert.ToString(""));

            templateHTML = templateHTML.Replace("{{HCT}}", Convert.ToString(tr.PHCTAVG));
            templateHTML = templateHTML.Replace("{{HCT_FG}}", hctFlag);
            templateHTML = templateHTML.Replace("{{HCT_REF_RG}}", hctReferenceRange);
            templateHTML = templateHTML.Replace("{{HCT_NT}}", Convert.ToString(""));

            templateHTML = templateHTML.Replace("{{NHCT}}", Convert.ToString(tr.NHCTAVG));
            templateHTML = templateHTML.Replace("{{NHCT_FG}}", nhctFlag);
            templateHTML = templateHTML.Replace("{{NHCT_REF_RG}}", nhctReferenceRange);
            templateHTML = templateHTML.Replace("{{NHCT_NT}}", Convert.ToString(""));

            templateHTML = templateHTML.Replace("{{AL_TR}}", Convert.ToString(albuminTransudation) + "%");
            templateHTML = templateHTML.Replace("{{AL_TR_FG}}", albuminTransudationFlag);
            templateHTML = templateHTML.Replace("{{AL_TR_REF_RG}}", albuminTransudationReferenceRange);
            templateHTML = templateHTML.Replace("{{AL_TR_NT}}", albuminTransudationNotes);

            templateHTML = templateHTML.Replace("{{ORDERING_PHYS}}", Convert.ToString(tr.OrderingPhysician));
            templateHTML = templateHTML.Replace("{{TECH}}", Convert.ToString(tr.FirstName + " " + tr.LastName));
            templateHTML = templateHTML.Replace("{{INJ_LT_NUM}}", Convert.ToString(tr.InjectateLotNumber));

            byte[] pdf = ConvertHTMLStringToPDF(templateHTML);
            return pdf;

        }

        public static byte[] BuildQCLogPDF(QCLogReportModel ql)
        {
            string templateHTML = File.ReadAllText("./Resources/qclog_template.html");
            templateHTML = templateHTML.Replace("{{QC_DATE}}", ql.Timestamp.ToString());
            templateHTML = templateHTML.Replace("{{TEST_TYPE}}", ql.TestType);
            templateHTML = templateHTML.Replace("{{COUNTS}}", ql.Counts.ToString());
            templateHTML = templateHTML.Replace("{{CPM}}", ql.CPM.ToString());
            templateHTML = templateHTML.Replace("{{RESULT}}", ql.Result == true ? "Pass" : "Fail");
            templateHTML = templateHTML.Replace("{{PREFORMED_BY}}", ql.PreformedBy);
            byte[] pdf = ConvertHTMLStringToPDF(templateHTML);
            return pdf;
        }

        public static string ExportDocumentToFlashDrive(string usbPath, string filename, string format, byte[] exportStream)
        {
            filename = filename.Replace(" ", "_").Replace("/", "_").Replace(":", "_");
            usbPath += filename + "." + format.ToLower();
            try
            {
                System.IO.File.WriteAllBytes(usbPath, exportStream);
                return usbPath;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to export file(s) to USB Drive");
            }
        }

        public static byte[] ConvertHTMLStringToPDF(string html)
        {
            string tmpOutFileName = Guid.NewGuid().ToString() + ".html";
            string tmpInFileName = tmpOutFileName.Replace(".html", ".pdf");
            string path = Directory.GetCurrentDirectory();
            string cmd = "";
            byte[] pdf = null;
            string outPath = "";
            string inPath = "";
            if (IsLinux())
            {
                outPath = path + "/Binaries/Linux/" + tmpOutFileName;
                inPath = path + "/Binaries/Linux/" + tmpInFileName;
                System.IO.File.WriteAllText(outPath, html);
                cmd = "wkhtmltopdf " + outPath + " " + inPath;
                RunLinuxCommand(cmd);
                while (!File.Exists(inPath))
                {
                    Thread.Sleep(1000);
                }
                pdf = File.ReadAllBytes(path + "/Binaries/Linux/" + tmpInFileName);
                File.Delete(path + "/Binaries/Linux/" + tmpInFileName);
                File.Delete(path + "/Binaries/Linux/" + tmpOutFileName);
                return pdf;
            }
            else
            {
                outPath = path + "\\Binaries\\Windows\\" + tmpOutFileName;
                inPath = path + "\\Binaries\\Windows\\" + tmpInFileName;
                System.IO.File.WriteAllText(outPath, html);
                cmd = path + "\\Binaries\\Windows\\wkhtmltopdf.exe " + outPath + " " + inPath;
                RunWindowsCommand(cmd);
                while (!File.Exists(inPath))
                {
                    Thread.Sleep(1000);
                }
                pdf = File.ReadAllBytes(path + "\\Binaries\\Windows\\" + tmpInFileName);
                File.Delete(path + "\\Binaries\\Windows\\" + tmpInFileName);
                File.Delete(path + "\\Binaries\\Windows\\" + tmpOutFileName);
                return pdf;
            }
        }
    }
}
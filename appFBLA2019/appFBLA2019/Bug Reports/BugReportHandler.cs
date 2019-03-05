//BizQuiz App 2019

using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace appFBLA2019
{
    public static class BugReportHandler
    {
        /// <summary>
        /// Location of the realm where bug reports are stored
        /// </summary>
        private static string BugRealmPath { get { return App.Path + "/bugreports.realm"; } }

        /// <summary>
        /// Makes sure the bug report images folder exists
        /// </summary>
        public static void Setup()
        {
            Directory.CreateDirectory(App.Path + "/bugreportimages");
        }

        /// <summary>
        /// If there is a crash log saved, processes it into a bug report and saves it to realm
        /// </summary>
        public static void ProcessCrashLog()
        {
            string logPath = App.Path + "/CrashReport.txt";
            if (File.Exists(logPath))
            {
                var errorText = File.ReadAllText(logPath);
                SaveReport(new BugReport("Unhandled Exception", "Exceptions", errorText));
                File.Delete(logPath);
            }
        }

        /// <summary>
        /// Given an exception, saves it as a bug report with information about the location of the exception
        /// </summary>
        /// <param name="exception">The exception to save</param>
        /// <param name="source">Where the exception came from (optional)</param>
        /// <param name="parentFile">for internal use</param>
        /// <param name="callLine">for internal use</param>
        /// <param name="parentMethod">for internal use</param>
        public static void SaveReport(Exception exception, string source = "", [CallerFilePath]string parentFile = "", [CallerLineNumber]int callLine = 0, [CallerMemberName]string parentMethod = "")
        {
            if (source == "")
            {
                source = $"Method {parentMethod} in {parentFile} at line {callLine}";
            }
            SaveReport(new BugReport($"Unhandled Exception from {source}", $"{source} Exceptions", exception.ToString()));
        }

        /// <summary>
        /// Saves a bugreport to realm, copying its image to bugreportimages if applicable
        /// </summary>
        /// <param name="report">The report to save</param>
        public static void SaveReport(BugReport report)
        {
            Realms.Realm bugRealm = Realms.Realm.GetInstance(new Realms.RealmConfiguration(BugRealmPath));
            //if no already saved reports have the same hash (same contents)
            if (bugRealm?.All<BugReport>().Where<BugReport>(x => x.ReportID == report.ReportID).ToList().Count == 0)
            {
                bugRealm.Write(() =>
                {
                    if (report.ImagePath != null && report.ImagePath != "")
                    {
                        byte[] imageByteArray = File.ReadAllBytes(report.ImagePath);
                        File.WriteAllBytes(App.Path + $"/bugreportimages/{report.ReportID}.jpg", imageByteArray);
                        report.ImagePath = App.Path + $"/bugreportimages/{report.ReportID}.jpg";
                    }
                    bugRealm.Add(report, true);
                });
            }
        }

        /// <summary>
        /// Submits all reports from the local realm to the server
        /// </summary>
        public static void SubmitSavedReports()
        {
            Realms.Realm bugRealm = Realms.Realm.GetInstance(new Realms.RealmConfiguration(BugRealmPath));
            if (bugRealm.All<BugReport>().ToList().Count > 0)
            {
                foreach (BugReport report in bugRealm.All<BugReport>().ToList())
                {
                    if (SendReport(report))
                    {
                        if (report.ImagePath != null)
                        {
                            File.Delete(report.ImagePath);
                        }
                        bugRealm.Write(() => bugRealm.Remove(report));
                    }
                }
            }
        }

        /// <summary>
        /// Sends a bug report to the server
        /// </summary>
        /// <param name="report">The report to send</param>
        /// <returns>If the bug report was sent successfully or not</returns>
        private static bool SendReport(BugReport report)
        {
            byte[] image;
            try
            {
                image = File.ReadAllBytes(report.ImagePath);
            }
            catch
            {
                image = null;
            }

            //sends the report to the server (and the image, provided it's not null)
            //returns if the send was successful or not
            return ServerOperations.SendBugReport(report.ToString(), image);
        }
    }
}
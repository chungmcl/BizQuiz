//BizQuiz App 2019

using System;
using System.IO;
using System.Linq;

namespace appFBLA2019
{
    public static class BugReportHandler
    {
        private static string BugRealmPath { get { return App.Path + "/bugreports.realm"; } }

        public static void Setup()
        {
            Directory.CreateDirectory(App.Path + "/bugreportimages");
        }

        public static void ProcessCrashLog()
        {
            string logPath = App.Path + "/CrashReport.txt";
            if (File.Exists(logPath))
            {
                var errorText = File.ReadAllText(logPath);
                BugReportHandler.SaveReport(new BugReport("Unhandled Exception", "Exceptions", errorText));
                File.Delete(logPath);
            }
        }

        public static void SaveReport(Exception exception, string source)
        {
            SaveReport(new BugReport($"Unhandled Exception from {source}", $"{source} Exceptions", exception.ToString()));
        }

        public static void SaveReport(Exception exception)
        {
            SaveReport(exception, "Internal code");
        }

        /// <summary>
        /// Attempts to save a bug report to the local directory
        /// </summary>
        /// <param name="report">  </param>
        /// <returns>  </returns>
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

        //micheal write in this one
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
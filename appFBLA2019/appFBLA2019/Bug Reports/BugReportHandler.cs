//BizQuiz App 2019

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace appFBLA2019
{
    public static class BugReportHandler
    {
        public static void Setup()
        {
            bugRealm = Realms.Realm.GetInstance(App.Path + "/bugreports.realm");
            Directory.CreateDirectory(App.Path + "/bugreportimages");
        }

        public static bool SubmitReport(BugReport report)
        {
            //if there are no saved reports with identical contents
            if (bugRealm?.All<BugReport>().Where<BugReport>(x => x.ReportID == report.ReportID).ToList().Count == 0)
            {
                if (SendReport(report))
                {
                    return true;
                }
                else
                {
                    SaveBugReport(report);
                    return false;
                }
            }
            return false;
        }

        public static bool SubmitReport(Exception exception, string source)
        {
            return SubmitReport(new BugReport($"Unhandled Exception from {source}", $"{source} Exceptions", exception.ToString()));
        }

        public static bool SubmitReport(Exception exception)
        {
            return SubmitReport(exception, "Internal code");
        }

        /// <summary>
        /// Attempts to save a bug report to the local directory
        /// </summary>
        /// <param name="report">  </param>
        /// <returns>  </returns>
        private static void SaveBugReport(BugReport report)
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

        private static Realms.Realm bugRealm;

        public static void SubmitSavedReports()
        {
            if (bugRealm.All<BugReport>().ToList().Count > 0)
            {
                foreach (BugReport report in bugRealm.All<BugReport>().ToList())
                {
                    if (BugReportHandler.SubmitReport(report))
                    {
                        File.Delete(report.ImagePath);
                        bugRealm.Write(() =>
                            bugRealm.Remove(report));
                    }
                }
            }
        }

        //micheal write in this one
        private static bool SendReport(BugReport report)
        {
            string reportText = report.ToString();
            Image reportImage = new Image { Source = ImageSource.FromFile(report.ImagePath) };

            //sends the report to the server (and the image, provided it's not null)
            //returns if the send was successful or not
            return false;
        }
    }
}
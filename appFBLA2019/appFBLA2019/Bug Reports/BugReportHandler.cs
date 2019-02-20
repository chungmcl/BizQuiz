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
    internal static class BugReportHandler
    {
        public static void Setup()
        {
            bugRealm = Realms.Realm.GetInstance(App.Path + "/bugreports.realm");
            Directory.CreateDirectory(App.Path + "/bugreportimages");
        }

        public static bool SubmitReport(BugReport report)
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

        /// <summary>
        /// Attempts to save a bug report to the local directory
        /// </summary>
        /// <param name="report">  </param>
        /// <returns>  </returns>
        private static void SaveBugReport(BugReport report)
        {
            try
            {
                bugRealm.Write(() =>
               {
                   byte[] imageByteArray = File.ReadAllBytes(report.ImagePath);
                   File.WriteAllBytes(App.Path + $"/bugreportimages/{report.ReportID}.jpg", imageByteArray);
                   report.ImagePath = App.Path + $"/bugreportimages/{report.ReportID}.jpg";
                   bugRealm.Add(report, true);
               });
            }
            catch (Exception ex)// If the database failed to connect
            {
                string test = ex.Message.ToString();
            }
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
            throw new NotImplementedException();
        }
    }
}
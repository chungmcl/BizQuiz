//BizQuiz App 2019

using System;
using System.Collections.Generic;
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
            bugRealm = Realms.Realm.GetInstance(App.Path + "bugreports");
        }

        public static bool SubmitReport(BugReport report)
        {
            if (SendReport(report.ToString(), report.image))
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
        private static bool SaveBugReport(BugReport report)
        {
            try
            {
                bugRealm.Add(report, true);
            }
            catch (Exception ex)// If the database failed to connect
            {
                string test = ex.Message.ToString();
                return false;
            }
            return true;
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
                        bugRealm.Remove(report);
                    }
                }
            }
        }

        //micheal write in this one
        private static bool SendReport(string report, Image reportimage)
        {
            //sends the report to the server (and the image, provided it's not null)
            //returns if the send was successful or not
            throw new NotImplementedException();
        }
    }
}
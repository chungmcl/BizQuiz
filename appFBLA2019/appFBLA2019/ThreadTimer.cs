using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace appFBLA2019
{
    /// <summary>
    /// Class to handle running timed task in background.
    /// </summary>
    public static class ThreadTimer
    {
        /// <summary>
        /// Update local data through synchronzing data with server.
        /// Runs on a timed background task and repeats a synchronization every two minutes.
        /// </summary>
        /// <returns></returns>
        public static async Task RunServerChecks()
        {
            await CredentialManager.CheckLoginStatus();
            await Task.Run(async() => await QuizRosterDatabase.UpdateLocalDatabase());
            var minutes = TimeSpan.FromMinutes(2);
            Device.StartTimer(minutes, () =>
            {
                Task task = Task.Run(async() =>
                {
                    if (CredentialManager.IsLoggedIn)
                    {
                        await CredentialManager.CheckLoginStatus();
                    }
                    
                    await QuizRosterDatabase.UpdateLocalDatabase();
                    BugReportHandler.SubmitSavedReports();
                });
                // Return true to continue the timer
                return true;
            });
        }
    }
}

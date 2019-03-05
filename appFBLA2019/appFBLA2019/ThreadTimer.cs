using System;
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
        public static async Task RunServerChecksAsync()
        {
            await CredentialManager.CheckLoginStatusAsync();
            await Task.Run(async() => await QuizRosterDatabase.UpdateLocalDatabaseAsync());
            var minutes = TimeSpan.FromMinutes(2);
            Device.StartTimer(minutes, () =>
            {
                Task task = Task.Run(async() =>
                {
                    if (CredentialManager.IsLoggedIn)
                    {
                        await CredentialManager.CheckLoginStatusAsync();
                    }
                    
                    await QuizRosterDatabase.UpdateLocalDatabaseAsync();
                    BugReportHandler.SubmitSavedReports();
                });
                // Return true to continue the timer
                return true;
            });
        }
    }
}

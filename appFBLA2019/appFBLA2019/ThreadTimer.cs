﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace appFBLA2019
{
    public static class ThreadTimer
    {
        public static async Task RunServerChecks()
        {
            await CredentialManager.CheckLoginStatus();
            await Task.Run(async() => await LevelRosterDatabase.UpdateLocalDatabase());
            var minutes = TimeSpan.FromMinutes(2);
            Device.StartTimer(minutes, () =>
            {
                Task task = Task.Run(async() =>
                {
                    if (CredentialManager.IsLoggedIn)
                    {
                        await CredentialManager.CheckLoginStatus();
                    }
                    
                    await LevelRosterDatabase.UpdateLocalDatabase();
                    BugReportHandler.SubmitSavedReports();
                });
                // Return true to continue the timer
                return true;
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace appFBLA2019
{
    public static class ThreadTimer
    {
        public static void RunServerChecks()
        {
            LevelRosterDatabase.LoadLevelInfos();
            LevelRosterDatabase.UpdateLocalDatabase();

            var minutes = TimeSpan.FromMinutes(2.0);
            Device.StartTimer(minutes, () =>
            {
                Task.Run(async () =>
                {
                    if (CredentialManager.IsLoggedIn)
                        await CredentialManager.CheckLoginStatus();

                    //LevelRosterDatabase.LoadLevelInfos();
                    //LevelRosterDatabase.UpdateLocalDatabase();
                });

                // Return true to continue the timer
                return true;
            });
        }
    }
}

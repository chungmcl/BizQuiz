using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace appFBLA2019
{
    public static class CredentialManager
    {
        public static string Username{ get; private set; }
        public static bool IsLoggedIn { get; private set; }

        public static void SaveCredential(string username, string password)
        {
            Username = username;

            Task.Run(async() => await SecureStorage.SetAsync("username", username));
            Task.Run(async() => await SecureStorage.SetAsync("password", password));

            IsLoggedIn = true;
        }

        public async static Task<OperationReturnMessage> CheckLoginStatus()
        {
            string username = await SecureStorage.GetAsync("username");
            string password = await SecureStorage.GetAsync("password");

            if (CrossConnectivity.Current.IsConnected)
            {
                if ((username != null) && (password != null))
                {
                    ServerConnector.SendData(ServerRequestTypes.LoginAccount, username + "/" + password + "/-");
                    OperationReturnMessage message = ServerConnector.ReceiveFromServerORM();
                    if (message == OperationReturnMessage.True || message == OperationReturnMessage.TrueConfirmEmail)
                        IsLoggedIn = true;
                    else
                        IsLoggedIn = false;
                    return message;
                }
                else
                {
                    return OperationReturnMessage.False;
                }
            }
            else
            {
                if ((username != null) && (password != null))
                {
                    return OperationReturnMessage.False;
                }
                else
                {
                    return OperationReturnMessage.True;
                }
            }
        }

        /// <summary>
        /// Check if the current saved login credentials match with the server - Is the user logged in?
        /// Checks every five minutes.
        /// Sets the IsLoggedIn property of CredentialManager.
        /// </summary>
        public static void StartTimedCheckLoginStatus()
        {
            var minutes = TimeSpan.FromMinutes(2.0);
            Device.StartTimer(minutes, () =>
            {
                Task.Run(async() =>
                {
                    if (IsLoggedIn)
                        await CheckLoginStatus();
                });
                
                // Return true to continue the timer
                return true;
            });
        }
    }
}

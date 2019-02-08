//BizQuiz App 2019

using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace appFBLA2019
{
    public static class CredentialManager
    {
        public static string Username
        {
            get; private set;
        }

        public static bool IsLoggedIn { get; private set; }

        public static void SaveCredential(string username, string password)
        {
            Username = username;

            Task.Run(async () => await SecureStorage.SetAsync("username", username));
            Task.Run(async () => await SecureStorage.SetAsync("password", password));

            IsLoggedIn = true;
        }

        public static async Task<OperationReturnMessage> CheckLoginStatus()
        {
            string username = await SecureStorage.GetAsync("username");
            Username = username;
            string password = await SecureStorage.GetAsync("password");

            if (CrossConnectivity.Current.IsConnected)
            {
                if (((username != null) && (password != null)) && ((username != "") && (password != "")))
                {
                    if (ServerConnector.SendData(ServerRequestTypes.LoginAccount, username + "/" + password + "/-"))
                    {
                        OperationReturnMessage message = ServerConnector.ReceiveFromServerORM();
                        if (message == OperationReturnMessage.True || message == OperationReturnMessage.TrueConfirmEmail)
                        {
                            IsLoggedIn = true;
                        }
                        else
                        {
                            IsLoggedIn = false;

                            await Task.Run(async () => await SecureStorage.SetAsync("password", ""));
                        }
                        return message;
                    }
                    else
                    {
                        return OperationReturnMessage.False;
                    }
                }
                else
                {
                    return OperationReturnMessage.False;
                }
            }
            else // If the user is offline
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
        /// Check if the current saved login credentials match with the server - Is the user logged in? Checks every two minutes. Sets the IsLoggedIn property of CredentialManager.
        /// </summary>
        public static void StartTimedCheckLoginStatus()
        {
            var minutes = TimeSpan.FromMinutes(2.0);
            Device.StartTimer(minutes, () =>
            {
                Task.Run(async () =>
                {
                    if (IsLoggedIn)
                    {
                        await CheckLoginStatus();
                    }
                });

                // Return true to continue the timer
                return true;
            });
        }
    }
}
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
        public static string Username { get; private set; }
        public static bool IsLoggedIn { get; private set; }
        public static bool EmailConfirmed { get; set; }
     
        public static void SaveCredential(string username, string password, bool emailConfirmed)
        {
            Username = username;

            Task.Run(async() => await SecureStorage.SetAsync("username", username));
            Task.Run(async() => await SecureStorage.SetAsync("password", password));

            IsLoggedIn = true;
            EmailConfirmed = emailConfirmed;
        }

        public static void Logout(bool clearUsername)
        {
            Task.Run(async () => await SecureStorage.SetAsync("password", ""));

            if (clearUsername)
                Task.Run(async () => await SecureStorage.SetAsync("username", ""));

            IsLoggedIn = false;
            EmailConfirmed = false;
        }

        public async static Task<OperationReturnMessage> CheckLoginStatus()
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
                        if (message == OperationReturnMessage.True)
                        {
                            IsLoggedIn = true;
                            EmailConfirmed = true;
                        }
                        else if (message == OperationReturnMessage.TrueConfirmEmail)
                        {
                            IsLoggedIn = true;
                            EmailConfirmed = false;
                        }
                        else
                        {
                            IsLoggedIn = false;
                            EmailConfirmed = false;

                            await SecureStorage.SetAsync("password", "");
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
                if (((username != null) && (password != null)) && ((username != "") && (password != "")))
                {
                    return OperationReturnMessage.False;
                }
                else
                {
                    return OperationReturnMessage.True;
                }
            }
        }
    }
}

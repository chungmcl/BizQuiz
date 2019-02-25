//BizQuiz App 2019

using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace appFBLA2019
{
    public static class CredentialManager
    {
        private static string username;
        public static string Username
        {
            get
            {
                return username;
            }
            set
            {
                username = value;
                Directory.CreateDirectory(App.Path + "/" + value);
            }
        }

        public static string Password { get; private set; }
        public static bool IsLoggedIn { get; set; }
        public static bool EmailConfirmed { get; set; }

        public static void SaveCredential(string username, string password, bool emailConfirmed)
        {
            Task.Run(async () => await SecureStorage.SetAsync("username", username));
            Task.Run(async () => await SecureStorage.SetAsync("password", password));
            
            Username = username;
            Password = password;

            IsLoggedIn = true;
            EmailConfirmed = emailConfirmed;
        }

        public static void Logout(bool clearUsername)
        {
            Task.Run(async () => await SecureStorage.SetAsync("password", ""));

            if (clearUsername)
            {
                Task.Run(async () => await SecureStorage.SetAsync("username", ""));
            }

            Username = "dflt";
            IsLoggedIn = false;
            EmailConfirmed = false;
        }

        public static async Task<OperationReturnMessage> CheckLoginStatus()
        {
            string username = await SecureStorage.GetAsync("username");
            Username = username;
            string password = await SecureStorage.GetAsync("password");
            Password = password;

            if (CrossConnectivity.Current.IsConnected)
            {
                if (((username != null) && (password != null)) && ((username != "") && (password != "")))
                {
                    OperationReturnMessage message = ServerOperations.LoginAccount(username, password);
                    if (message != OperationReturnMessage.FalseFailedConnection)
                    {
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
                            Username = "dflt";
                            await SecureStorage.SetAsync("password", "");
                        }
                        return message;
                    }
                    else
                    {
                        return CannotConnectToServer(username, password);
                    }
                }
                else
                {

                    Username = "dflt";
                    IsLoggedIn = false;
                    EmailConfirmed = false;
                    Username = "dflt";
                    Password = "";
                    return OperationReturnMessage.False;
                }
            }
            else // If the user is offline
            {
                return CannotConnectToServer(username, password);
            }
        }

        private static OperationReturnMessage CannotConnectToServer(string username, string password)
        {
            if (((username != null) && (password != null)) && ((username != "") && (password != "")))
            {
                IsLoggedIn = true;
                Username = username;
                return OperationReturnMessage.True;
            }
            else
            {
                Username = "dflt";
                IsLoggedIn = false;
                EmailConfirmed = false;
                Password = "";
                return OperationReturnMessage.False;
            }
        }
    }
}
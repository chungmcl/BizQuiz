//BizQuiz App 2019

using Plugin.Connectivity;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace appFBLA2019
{
    public static class CredentialManager
    {
        /// <summary>
        /// private field for Username property.
        /// </summary>
        private static string username;

        /// <summary>
        /// The username of the currently logged in user.
        /// Username is set to "dflt" if user is not logged in.
        /// </summary>
        public static string Username
        {
            get
            {
                
                return username ?? "dflt";
            }
            set
            {
                username = value;
                // Create a directory for the user locally for saved quizzes.
                Directory.CreateDirectory(App.Path + "/" + value);
            }
        }

        /// <summary>
        /// Whether the user is logged in or not.
        /// </summary>
        public static bool IsLoggedIn { get; set; }
        /// <summary>
        /// Whether the user has had their email confirmed.
        /// </summary>
        public static bool EmailConfirmed { get; set; }

        /// <summary>
        /// Save the user's credentials securely to app Keystore.
        /// </summary>
        /// <param name="username">The username of the account.</param>
        /// <param name="password">The password of the account.</param>
        /// <param name="emailConfirmed">Whether the user has had their email confirmed or not.</param>
        public static async Task SaveCredentialAsync(string username, string password, bool emailConfirmed)
        {
            await SecureStorage.SetAsync("username", username);
            await SecureStorage.SetAsync("password", password);
            
            Username = username;

            IsLoggedIn = true;
            EmailConfirmed = emailConfirmed;

            if (Directory.GetDirectories(App.UserPath).Length < 5)
            {
                await DependencyService.Get<IGetStorage>().SetupDefaultQuizzesAsync(App.UserPath);
            }
        }

        /// <summary>
        /// Log the user out of the account.
        /// </summary>
        /// <param name="clearUsername">Whether to clear the username field in the Keystore or not.</param>
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

        /// <summary>
        /// Check if the current user credentials are valid with the server or not
        /// in a background task.
        /// </summary>
        /// <returns>The OperationReturnMessage from the server.</returns>
        public static async Task<OperationReturnMessage> CheckLoginStatusAsync()
        {
            string username = await SecureStorage.GetAsync("username");
            Username = username;

            if (CrossConnectivity.Current.IsConnected)
            {
                if ((username != null) && (username != ""))
                {
                    OperationReturnMessage message = ServerOperations.LoginAccount(username, await SecureStorage.GetAsync("password"));
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
                        return CannotConnectToServer(username);
                    }
                }
                else
                {

                    Username = "dflt";
                    IsLoggedIn = false;
                    EmailConfirmed = false;
                    Username = "dflt";
                    return OperationReturnMessage.False;
                }
            }
            else // If the user is offline
            {
                return CannotConnectToServer(username);
            }
        }

        /// <summary>
        /// Handle credentials if the app could not connect to server.
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns></returns>
        private static OperationReturnMessage CannotConnectToServer(string username)
        {
            if ((username != null) && (username != ""))
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
                return OperationReturnMessage.False;
            }
        }
    }
}
//BizQuiz App 2019

using Plugin.Connectivity;
using Realms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace appFBLA2019
{
    public static class ServerOperations
    {
        private const int stringHeaderSize = 5;
        private const int imageHeaderSize = maxUsernameSize + maxPasswordSize + maxImageNameSize + maxDBIdSize;
        private const int maxImageNameSize = 72; // 36 char GUID (without file extension)
        private const int maxDBIdSize = 72; // 36 char GUID
        private const string jpegFileExtension = ".jpg";

        private const int realmHeaderSize = maxUsernameSize + maxPasswordSize;

        private const int maxUsernameSize = 64;
        private const int maxPasswordSize = 64;
        private const string realmFileExtension = ".realm";

        private const int headerSize = 5;

        /// <summary>
        /// Attempt to login to account through server.
        /// </summary>
        /// <param name="username">The username of the account.</param>
        /// <param name="password">The password of the account.</param>
        /// <returns>The OperationReturnMessage the server returns.</returns>
        public static OperationReturnMessage LoginAccount(string username, string password)
        {
            return (OperationReturnMessage)SendStringData($"{username}/{password}/-", ServerRequestTypes.LoginAccount);
        }

        /// <summary>
        /// Attempt to register account through server.
        /// </summary>
        /// <param name="username">The username of the account</param>
        /// <param name="password">The password of the account.</param>
        /// <param name="email">The email of the account.</param>
        /// <returns>The OperationReturnMessage the server returns.</returns>
        public static OperationReturnMessage RegisterAccount(string username, string password, string email)
        {
            return (OperationReturnMessage)SendStringData($"{username}/{password}/{email}/-", ServerRequestTypes.RegisterAccount);
        }

        /// <summary>
        /// Change the password of an account through server.
        /// </summary>
        /// <param name="username">The current username.</param>
        /// <param name="currentPassword">The current password.</param>
        /// <param name="newPassword">The new password to change password to.</param>
        /// <returns>The OperationReturnMessage the server returns.</returns>
        public static OperationReturnMessage ChangePassword(string username, string currentPassword, string newPassword)
        {
            return (OperationReturnMessage)SendStringData($"{username}/{currentPassword}/{newPassword}/-", ServerRequestTypes.ChangePassword);
        }

        /// <summary>
        /// Confirm an email through server.
        /// </summary>
        /// <param name="username">The username of the account to confirm.</param>
        /// <param name="confirmationToken">The token sent to the associated email to confirm email.</param>
        /// <returns>The OperationReturnMessage the server returns.</returns>
        public static OperationReturnMessage ConfirmEmail(string username, string confirmationToken)
        {
            return (OperationReturnMessage)SendStringData($"{username}/{confirmationToken}/-", ServerRequestTypes.ConfirmEmail);
        }

        /// <summary>
        /// Change an email through server.
        /// </summary>
        /// <param name="username">The username of the account associated with the email.</param>
        /// <param name="password">The password of the account.</param>
        /// <param name="newEmail">The new email to change email to.</param>
        /// <returns>The OperationReturnMessage the server returns.</returns>
        public static OperationReturnMessage ChangeEmail(string username, string password, string newEmail)
        {
            return (OperationReturnMessage)SendStringData($"{username}/{password}/{newEmail}/-", ServerRequestTypes.ChangeEmail);
        }

        /// <summary>
        /// Delete an account through server.
        /// </summary>
        /// <param name="username">The username fo the account to delete.</param>
        /// <param name="password">The password of the account to delete.</param>
        /// <returns>The OperationReturnMessage the server returns.</returns>
        public static OperationReturnMessage DeleteAccount(string username, string password)
        {
            return (OperationReturnMessage)SendStringData($"{username}/{password}/-", ServerRequestTypes.DeleteAccount);
        }

        /// <summary>
        /// Get the last modified date of a quiz given the quizzes's DBId.
        /// </summary>
        /// <param name="DBId">The DBId of the quiz to check.</param>
        /// <returns>The DateTime of the last modified date of a quiz as a string.</returns>
        public static string GetLastModifiedDate(string DBId)
        {
            return (string)SendStringData($"{DBId}/-", ServerRequestTypes.GetLastModifiedDate);
        }

        /// <summary>
        /// Get the email of an account.
        /// </summary>
        /// <param name="username">The username of the account.</param>
        /// <param name="password">The password of the account</param>
        /// <returns>The email of the account as a string.</returns>
        public static string GetEmail(string username, string password)
        {
            return (string)SendStringData($"{username}/{password}/-", ServerRequestTypes.GetEmail);
        }

        /// <summary>
        /// Delete a quiz on the server.
        /// Uses current logged in credentials to authenticate permissions.
        /// </summary>
        /// <param name="DBId">The DBId of the quiz to delete.</param>
        /// <returns>The OperationReturnMessage the server returns.</returns>
        public async static Task<OperationReturnMessage> DeleteQuiz(string DBId)
        {
            return (OperationReturnMessage)SendStringData($"{CredentialManager.Username}/{await SecureStorage.GetAsync("password")}/{DBId}", ServerRequestTypes.DeleteQuiz);
        }

        /// <summary>
        /// Get quiz information based on a query string associated with the authorname.
        /// </summary>
        /// <param name="authorName">The author name to query for.</param>
        /// <param name="chunk">The section of data to retrieve from server - server returns 20 quiz data at a time.</param>
        /// <returns>A list of string arrays detailing quiz data based on the query string.</returns>
        public static List<string[]> GetQuizzesByAuthorName(string authorName, int chunk)
        {
            return (List<string[]>)SendStringData($"{authorName}/{chunk}/-", ServerRequestTypes.GetQuizzesByAuthorName);
        }

        /// <summary>
        /// Get quiz information of quizzes not on device of current account based on a query string associated with the authorname.
        /// </summary>
        /// <param name="authorName">The author name to query for.</param>
        /// <param name="dBIdsOnDevice">The total number of quizzes currently on device.</param>
        /// <returns>A list of string arrays detailing quiz data based on the query string.</returns>
        public static List<string[]> GetMissingQuizzesByAuthorName(string authorName, string[] dBIdsOnDevice)
        {
            string queryString = $"{authorName}";
            foreach (string dbId in dBIdsOnDevice)
                queryString = queryString + "/" + dbId;
            return (List<string[]>)SendStringData(queryString, ServerRequestTypes.GetMissingQuizzesByAuthorName);
        }

        /// <summary>
        /// Get a list of users based on query string of username.
        /// </summary>
        /// <param name="username">The username to query for.</param>
        /// <param name="chunk">The section of data to retrieve from server - server returns 20 quiz data at a time.</param>
        /// <returns>A list of string arrays user data based on the query string.</returns>
        public static List<string[]> GetUsers(string username, int chunk)
        {
            return (List<string[]>)SendStringData($"{username}/{chunk}/-", ServerRequestTypes.GetUsers);
        }

        /// <summary>
        /// Get quiz data of quizzes by category.
        /// </summary>
        /// <param name="category">The category name to query for.</param>
        /// <param name="chunk">The section of data to retrieve from server - server returns 20 quiz data at a time.</param>
        /// <returns>A list of string arrays detailing quiz data based on the query string.</returns>
        public static List<string[]> GetQuizzesByCategory(string category, int chunk)
        {
            return (List<string[]>)SendStringData($"{category}/{chunk}/-", ServerRequestTypes.GetQuizzesByCategory);
        }

        /// <summary>
        /// Get quiz data of quizzes by quiz name.
        /// </summary>
        /// <param name="quizName">The quiz name to query for.</param>
        /// <param name="chunk">The section of data to retrieve from server - server returns 20 quiz data at a time.</param>
        /// <returns></returns>
        public static List<string[]> GetQuizzesByQuizName(string quizName, int chunk)
        {
            return (List<string[]>)SendStringData($"{quizName}/{chunk}/-", ServerRequestTypes.GetQuizzesByQuizName);
        }
        
        /// <summary>
        /// Requests server for a count of how many quizzes the user has created.
        /// Returns -1 if fails to convert to server.
        /// </summary>
        /// <param name="username">The author name to query for.</param>
        /// <returns>An integer detailing the number of quizzes by an author.</returns>
        public static int GetNumberOfQuizzesByAuthorName(string username)
        {
            string returnData = (string)SendStringData($"{username}/-", ServerRequestTypes.GetNumberOfQuizzesByAuthorName);
            if (!int.TryParse(returnData, out int result))
            {
                result = -1;
            }
            return result;
        }

        /// <summary>
        /// Subscribe to a quiz given the quiz's DBId.
        /// Utilizes current logged in credentials to authenticate permissions.
        /// </summary>
        /// <param name="dbId">The DBId of the quiz to subscribe to.</param>
        /// <returns>The OperationReturnMessage the server returns.</returns>
        public static async Task<OperationReturnMessage> SubscribeToQuiz(string dbId)
        {
            return (OperationReturnMessage)SendStringData($"{CredentialManager.Username}/{await SecureStorage.GetAsync("password")}/{dbId}/-", 
                ServerRequestTypes.SubscribeToQuiz);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbId"></param>
        /// <returns>The OperationReturnMessage the server returns.</returns>
        public static async Task<OperationReturnMessage> UnsubscribeToQuizAsync(string dbId)
        {
            return (OperationReturnMessage)SendStringData($"{CredentialManager.Username}/{await SecureStorage.GetAsync("password")}/{dbId}/-", 
                ServerRequestTypes.UnsubscribeToQuiz);
        }

        /// <summary>
        /// Send a quiz to the server with the current user credentials.
        /// </summary>
        /// <param name="relativeQuizPath">The path to the quiz on the current device.</param>
        /// <returns>A bool wrapped in a task representing whether the quiz successfully sent or not.</returns>
        public async static Task<bool> SendQuiz(string relativeQuizPath)
        {
            try
            {
                string realmFilePath = Directory.GetFiles(App.UserPath + relativeQuizPath, "*.realm").First();
                Realm realm = Realm.GetInstance(new RealmConfiguration(realmFilePath));
                QuizInfo info = realm.All<QuizInfo>().First();
                
                if (await SendRealmFile(realmFilePath) != OperationReturnMessage.True)
                {
                    throw new Exception();
                }

                string[] imageFilePaths = Directory.GetFiles(App.UserPath + relativeQuizPath, "*.jpg");
                for (int i = 0; i < imageFilePaths.Length; i++)
                {
                    //[0] = path, [1] = fileName, [2] = dBId
                    string fileName = imageFilePaths[i].Split('/').Last().Split('.').First();
                    string dbID = info.DBId;
                    OperationReturnMessage message = await SendImageFile(imageFilePaths[i], fileName, dbID);
                    
                    if (message == OperationReturnMessage.False)
                    {
                        throw new Exception();
                    }
                }

                // When finished, confirm with server that quiz send has completed
                OperationReturnMessage finalizationMessage = (OperationReturnMessage)SendStringData(
                    $"{info.DBId}`{info.LastModifiedDate}`{imageFilePaths.Length + 1}`" +
                    $"{CredentialManager.Username}`{await SecureStorage.GetAsync("password")}`-",
                    ServerRequestTypes.FinalizeQuizSend);

                if (finalizationMessage == OperationReturnMessage.True)
                {
                    QuizInfo infoCopy = new QuizInfo(info)
                    {
                        SyncStatus = 2
                    };
                    QuizRosterDatabase.EditQuizInfo(infoCopy);
                    return true;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                // Alert server that quiz send failed Delete records
                return false;
            }
        }

        /// <summary>
        /// Download a quiz from the server.
        /// </summary>
        /// <param name="dBId">The DBId of the quiz to download</param>
        /// <param name="quizName">The name of the quiz to download.</param>
        /// <param name="authorName">The name of the author of the quiz.</param>
        /// <param name="category">The category the quiz belongs in.</param>
        /// <returns></returns>
        public static bool GetQuiz(string dBId, string quizName, string authorName, string category)
        {
            // TO DO: CHECK IF quizName or category changed and save accordingly
            string tempPath = App.UserPath + "/" + "temp" + "/" + dBId + "/";
            string tempFilePath = tempPath + "/" + realmFileExtension;
            
            // Store the realm file into a temporary directory in order the retrieve information from it
            // (Information regarding the images in order to retrieve them from the server)
            Directory.CreateDirectory(tempPath);
            byte[] realmFile = (byte[])SendStringData($"{dBId}/-", ServerRequestTypes.GetRealmFile);
            if (realmFile.Length > 0)
                File.WriteAllBytes(tempFilePath, realmFile);
            else
                return false;

            // Retrieve info regarding the images from the realm file from its temporary location
            Realm realmDB = Realm.GetInstance(new RealmConfiguration(tempFilePath));
            IQueryable<Question> questionsWithPictures = realmDB.All<Question>().Where(question => question.NeedsPicture);
            foreach (Question question in questionsWithPictures)
            {
                byte[] jpegFile = (byte[])SendStringData($"{question.QuestionId}/{dBId}/-", ServerRequestTypes.GetJPEGImage);
                string jpegFilePath = tempPath + "/" + question.QuestionId + jpegFileExtension;
                if (jpegFile.Length > 0)
                    File.WriteAllBytes(jpegFilePath, jpegFile);
                else
                    return false;
            }
            
            QuizInfo info = realmDB.All<QuizInfo>().First();
            string newQuizName = info.QuizName;
            string newCategory = info.Category;
            string newLastModfiedDate = info.LastModifiedDate;

            string quizPath = App.UserPath + $"/{newCategory}/{info.DBId}/";
            string realmFilePath = quizPath + "/" + info.DBId + realmFileExtension;
            Directory.CreateDirectory(quizPath);

            string[] imageFilePaths = Directory.GetFiles(tempPath, "*.jpg");
            string[] realmFilePaths = Directory.GetFiles(tempPath, "*.realm");
            foreach (string path in realmFilePaths)
                File.Copy(path, quizPath + "/" + newQuizName + realmFileExtension, true);

            foreach (string path in imageFilePaths)
            {
                string imageName = path.Split('/').Last();
                File.Copy(path, quizPath + "/" + imageName, true);
            }

            if (quizName != newQuizName || category != newCategory)
                DeleteDirectory(App.UserPath + "/" + category + "/" + $"{quizName}`{authorName}", true);

            DeleteDirectory(tempPath, true);

            QuizInfo infoCopy = new QuizInfo(QuizRosterDatabase.GetQuizInfo(dBId))
            {
                SyncStatus = 2,
                QuizName = newQuizName,
                Category = newCategory,
                LastModifiedDate = newLastModfiedDate
            };
            QuizRosterDatabase.EditQuizInfo(infoCopy);

            return true;
        }

        /// <summary>
        /// Delete the specified directory.
        /// </summary>
        /// <param name="path">The path to the directory to delete.</param>
        /// <param name="recursive">Whether or not to recursively delete subfolders and files within the specified directory.</param>
        private static void DeleteDirectory(string path, bool recursive)
        {
            if (recursive)
            {
                var subfolders = Directory.GetDirectories(path);
                foreach (var s in subfolders)
                {
                    DeleteDirectory(s, recursive);
                }
            }
            var files = Directory.GetFiles(path);
            foreach (var f in files)
            {
                try
                {
                    var attr = File.GetAttributes(f);
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(f, attr ^ FileAttributes.ReadOnly);
                    }
                    File.Delete(f);
                }
                catch (IOException)
                {
                    Debug.WriteLine("failed delete");
                }
            }

            // At this point, all the files and sub-folders have been deleted.
            // So we delete the empty folder using the OOTB  DeleteDirectory method.
            Directory.Delete(path);
        }

        /// <summary>
        /// Send a bugreport to the server
        /// </summary>
        /// <param name="report">The exception or bug text to send.</param>
        /// <param name="image">The image associated with the bugreport to send.</param>
        /// <returns></returns>
        public static bool SendBugReport(string report, byte[] image = null)
        {
            byte[] reportBytes = Encoding.Unicode.GetBytes(report);
            byte[] imageBytes = image ?? new byte[0];
            byte[] imageHeader = new byte[5];
            if (image?.Length > 0)
            {
                imageHeader = new byte[5];
                Array.Copy(new byte[] { Convert.ToByte(true) }, imageHeader, 1);
                Array.Copy(BitConverter.GetBytes(imageBytes.Length), 0, imageHeader, 1, 4);
            }
            else { Array.Copy(new Byte[] { Convert.ToByte(false) }, imageHeader, 1); }
            byte[] header = GenerateHeaderData(ServerRequestTypes.SendBugReport, (uint)(imageHeader.Length + imageBytes.Length + reportBytes.Length));

            byte[] toSend = new byte[header.Length + imageHeader.Length + imageBytes.Length + reportBytes.Length];
            header.CopyTo(toSend, 0);
            imageHeader.CopyTo(toSend, header.Length);
            imageBytes.CopyTo(toSend, header.Length + imageHeader.Length);
            reportBytes.CopyTo(toSend, header.Length + imageHeader.Length + imageBytes.Length);

            switch (ReceiveFromServerORM(toSend))
            {
                case OperationReturnMessage.True:
                    return true;
                case OperationReturnMessage.False:
                case OperationReturnMessage.FalseFailedConnection:
                case OperationReturnMessage.FalseNoConnection:
                    return false;
                default:
                    throw new Exception("Something went wrong sending the bug report!");
            }
        }

        /// <summary>
        /// Request a forgot password code be sent to a user's email given the username.
        /// </summary>
        /// <param name="username">The username of the account that requires a forgot password code.</param>
        /// <returns></returns>
        public static OperationReturnMessage ForgotPassword(string username)
        {
            return (OperationReturnMessage)SendStringData($"{username}/-", ServerRequestTypes.ForgotPassword);
        }

        /// <summary>
        /// Change password if password is forgotten.
        /// </summary>
        /// <param name="username">The username that needs to have password changed.</param>
        /// <param name="resetCode">The reset code to confirm ownership of count that was sent to account's associated email</param>
        /// <param name="newPassword">The new password to change password to.</param>
        /// <returns></returns>
        public static OperationReturnMessage ForgotPasswordChangePassword(string username, string resetCode, string newPassword)
        {
            return (OperationReturnMessage)SendStringData($"{username}/{resetCode}/{newPassword}/-", ServerRequestTypes.ForgotPasswordChangePassword);
        }

        #region Header Generators
        /// <summary>
        /// Generate a header to detail what to do with data when server receives byte data.
        /// </summary>
        /// <param name="type">The type of server request to be performed.</param>
        /// <param name="size">The size of the data being sent to the server.</param>
        /// <returns></returns>
        private static byte[] GenerateHeaderData(ServerRequestTypes type, uint size)
        {
            byte[] headerData = new byte[stringHeaderSize];
            headerData[0] = (byte)type;
            byte[] dataSize = BitConverter.GetBytes(size);
            dataSize.CopyTo(headerData, 1);
            return headerData;
        }

        /// <summary>
        /// Generate a subheader for realm files.
        /// </summary>
        /// <returns>A byte array of the realm file subheader.</returns>
        private async static Task<byte[]> GenerateRealmHeader()
        {
            byte[] headerData = new byte[realmHeaderSize];

            byte[] username = Encoding.Unicode.GetBytes(CredentialManager.Username);
            Array.Copy(username, headerData, username.Length);

            byte[] password = Encoding.Unicode.GetBytes(await SecureStorage.GetAsync("password"));
            Array.Copy(password, 0, headerData, maxUsernameSize, password.Length);
            return headerData;
        }

        /// <summary>
        /// Generate a subheader for JPEG files.
        /// </summary>
        /// <param name="fileName">The filename of the jpg file to be sent.</param>
        /// <param name="dBId">The DBId the image is associated with.</param>
        /// <returns></returns>
        private async static Task<byte[]> GenerateImageHeader(string fileName, string dBId)
        {
            byte[] headerData = new byte[imageHeaderSize];

            byte[] username = Encoding.Unicode.GetBytes(CredentialManager.Username);
            Array.Copy(username, headerData, username.Length);

            byte[] password = Encoding.Unicode.GetBytes(await SecureStorage.GetAsync("password"));
            Array.Copy(password, 0, headerData, maxUsernameSize, password.Length);

            byte[] fileNameBytes = Encoding.Unicode.GetBytes(fileName);
            Array.Copy(fileNameBytes, 0, headerData, maxUsernameSize + maxPasswordSize, fileNameBytes.Length);

            byte[] dBIdBytes = Encoding.Unicode.GetBytes(dBId);
            Array.Copy(dBIdBytes, 0, headerData, maxUsernameSize + maxPasswordSize + maxImageNameSize, dBIdBytes.Length);
            return headerData;
        }
        #endregion

        #region Data Receives
        /// <summary>
        /// Receive an OperationReturnMessage from the server after sending a server request.
        /// </summary>
        /// <param name="toSend">The data to send to server.</param>
        /// <returns>The OperationReturnMessage returned by the server after data has been received
        /// and processed by the server.</returns>
        private static OperationReturnMessage ReceiveFromServerORM(byte[] toSend)
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                byte[] data = ServerConnector.SendByteArray(toSend);
                if (data.Length > 0)
                {
                    OperationReturnMessage message = (OperationReturnMessage)(data[0]);
                    return message;
                }
                else
                {
                    return OperationReturnMessage.FalseFailedConnection;
                }
            }
            else
            {
                return OperationReturnMessage.FalseNoConnection;
            }
        }

        /// <summary>
        /// Receive string data from the server after sending a server request.
        /// </summary>
        /// <param name="toSend">The data to send to server.</param>
        /// <returns>The string the server returns.</returns>
        private static string ReceiveFromServerStringData(byte[] toSend)
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                byte[] data = ServerConnector.SendByteArray(toSend);
                if (data.Length > 0)
                {
                    string dataString = Encoding.Unicode.GetString(data);
                    
                    dataString = dataString.Trim();
                    return dataString;
                }
                else
                {
                    
                    return null;
                }
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Receive list of string arrays from the server after sending a server request.
        /// </summary>
        /// <param name="toSend">The data to send to server.</param>
        /// <returns>The list of string arrays the server returns.</returns>
        private static List<string[]> ReceiveFromServerListOfStringArrays(byte[] toSend)
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                byte[] data = ServerConnector.SendByteArray(toSend);
                if (data.Length > 0)
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    MemoryStream memStream = new MemoryStream();
                    memStream.Write(data, 0, data.Length);
                    memStream.Position = 0;
                    
                    return binaryFormatter.Deserialize(memStream) as List<string[]>;
                }
                else
                {
                    
                    return new List<string[]>(0);
                }
            }
            else
            {
                return new List<string[]>(0);
            }
        }

        /// <summary>
        /// Receive 
        /// </summary>
        /// <param name="toSend"></param>
        /// <returns></returns>
        private static byte[] ReceiveFromServerBytes(byte[] toSend)
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                byte[] data = ServerConnector.SendByteArray(toSend);
                return data;
            }
            else
            {
                return new byte[0];
            }
        }
        #endregion

        #region Data Send Helper Methods
        /// <summary>
        /// Send a string (Unicode) based data request or send to the server.
        /// </summary>
        /// <param name="data">     The string data in Unicode to send. </param>
        /// <param name="dataType"> The type of request to send to the server. </param>
        private static object SendStringData(string data, ServerRequestTypes dataType)
        {
            string dataAsString = data;
            byte[] dataAsBytes = Encoding.Unicode.GetBytes(dataAsString);
            byte[] headerData = GenerateHeaderData(dataType, (uint)dataAsBytes.Length);
            byte[] toSend = new byte[headerData.Length + dataAsBytes.Length];
            headerData.CopyTo(toSend, 0);
            dataAsBytes.CopyTo(toSend, headerData.Length);

            switch (dataType)
            {
                case ServerRequestTypes.GetEmail:
                case ServerRequestTypes.GetLastModifiedDate:
                case ServerRequestTypes.GetNumberOfQuizzesByAuthorName:
                    return ReceiveFromServerStringData(toSend);

                case ServerRequestTypes.GetQuizzesByAuthorName:
                case ServerRequestTypes.GetQuizzesByQuizName:
                case ServerRequestTypes.GetQuizzesByCategory:
                case ServerRequestTypes.GetMissingQuizzesByAuthorName:
                    return ReceiveFromServerListOfStringArrays(toSend);

                case ServerRequestTypes.GetRealmFile:
                case ServerRequestTypes.GetJPEGImage:
                    return ReceiveFromServerBytes(toSend);
                    
                default:
                    return ReceiveFromServerORM(toSend);
            }
        }

        /// <summary>
        /// Send realm file to the server.
        /// </summary>
        /// <param name="path"> Path to the realm file on local device. </param>
        private async static Task<OperationReturnMessage> SendRealmFile(string path)
        {
            byte[] realmBytes = File.ReadAllBytes(path);
            byte[] realmHeader = await GenerateRealmHeader();
            byte[] header = GenerateHeaderData(ServerRequestTypes.AddRealmFile, (uint)realmBytes.Length + (uint)realmHeader.Length);

            byte[] toSend = new byte[header.Length + realmHeader.Length + realmBytes.Length];
            header.CopyTo(toSend, 0);
            realmHeader.CopyTo(toSend, header.Length);
            realmBytes.CopyTo(toSend, header.Length + realmHeader.Length);
            return ReceiveFromServerORM(toSend);
        }

        /// <summary>
        /// Send JPEG image to server given the local path, filename, and DBId the image is related to
        /// </summary>
        /// <param name="path">     Path to JPEG image file on local device. </param>
        /// <param name="fileName"> Name of the JPEG image file. </param>
        /// <param name="dbId">     DBId of the database image is contained in. </param>
        private async static Task<OperationReturnMessage> SendImageFile(string path, string fileName, string dbId)
        {
            byte[] imageBytes = File.ReadAllBytes(path);
            byte[] imageHeader = await GenerateImageHeader(fileName, dbId);
            byte[] header = GenerateHeaderData(ServerRequestTypes.AddJPEGImage, (uint)imageBytes.Length + (uint)imageHeader.Length);

            byte[] toSend = new byte[header.Length + imageHeader.Length + imageBytes.Length];
            header.CopyTo(toSend, 0);
            imageHeader.CopyTo(toSend, header.Length);
            imageBytes.CopyTo(toSend, header.Length + imageHeader.Length);
            return ReceiveFromServerORM(toSend);
        }
        #endregion
    }

    /// <summary>
    /// An enum detailing a message the server returns.
    /// Is stored as a single byte in byte array transmission from server.
    /// </summary>
    public enum OperationReturnMessage : byte
    {
        True,
        False,
        TrueConfirmEmail,
        FalseInvalidCredentials,
        FalseInvalidEmail,
        FalseUsernameAlreadyExists,
        FalseAlreadySubscribed,
        FalseAlreadyUnsubscribed,
        FalseNoConnection,
        FalseFailedConnection
    }

    /// <summary>
    /// An enum detailing the kind of action the client wants the server to perform.
    /// Is sent as the first byte in the byte array sent to server.
    /// </summary>
    public enum ServerRequestTypes : byte
    {
        // "Command" Requests
        StringData,
        AddJPEGImage,
        AddRealmFile,
        FinalizeQuizSend,
        SubscribeToQuiz,
        UnsubscribeToQuiz,
        DeleteQuiz,
        LoginAccount,
        RegisterAccount,
        DeleteAccount,
        ConfirmEmail,
        ChangeEmail,
        ChangePassword,
        SendBugReport,
        ForgotPassword,
        ForgotPasswordChangePassword,

        // "Get" Requests
        GetEmail,
        GetJPEGImage,
        GetRealmFile,
        GetLastModifiedDate,
        GetQuizzesByAuthorName,
        GetMissingQuizzesByAuthorName,
        GetQuizzesByQuizName,
        GetQuizzesByCategory,
        GetUsers,
        GetNumberOfQuizzesByAuthorName
    }
}
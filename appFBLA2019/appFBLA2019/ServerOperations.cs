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

        public static OperationReturnMessage LoginAccount(string username, string password)
        {
            return (OperationReturnMessage)SendStringData($"{username}/{password}/-", ServerRequestTypes.LoginAccount);
        }

        public static OperationReturnMessage RegisterAccount(string username, string password, string email)
        {
            return (OperationReturnMessage)SendStringData($"{username}/{password}/{email}/-", ServerRequestTypes.RegisterAccount);
        }

        public static OperationReturnMessage ChangePassword(string username, string currentPassword, string newPassword)
        {
            return (OperationReturnMessage)SendStringData($"{username}/{currentPassword}/{newPassword}/-", ServerRequestTypes.ChangePassword);
        }

        public static OperationReturnMessage ConfirmEmail(string username, string confirmationToken)
        {
            return (OperationReturnMessage)SendStringData($"{username}/{confirmationToken}/-", ServerRequestTypes.ConfirmEmail);
        }

        public static OperationReturnMessage ChangeEmail(string username, string password, string newEmail)
        {
            return (OperationReturnMessage)SendStringData($"{username}/{password}/{newEmail}/-", ServerRequestTypes.ChangeEmail);
        }

        public static OperationReturnMessage DeleteAccount(string username, string password)
        {
            return (OperationReturnMessage)SendStringData($"{username}/{password}/-", ServerRequestTypes.DeleteAccount);
        }

        public static string GetLastModifiedDate(string DBId)
        {
            return (string)SendStringData($"{DBId}/-", ServerRequestTypes.GetLastModifiedDate);
        }

        public static string GetEmail(string username, string password)
        {
            return (string)SendStringData($"{username}/{password}/-", ServerRequestTypes.GetEmail);
        }

        public async static Task<OperationReturnMessage> DeleteQuiz(string DBId)
        {
            return (OperationReturnMessage)SendStringData($"{CredentialManager.Username}/{await SecureStorage.GetAsync("password")}/{DBId}", ServerRequestTypes.DeleteQuiz);
        }

        public static List<string[]> GetQuizzesByAuthorName(string authorName, int chunk)
        {
            return (List<string[]>)SendStringData($"{authorName}/{chunk}/-", ServerRequestTypes.GetQuizzesByAuthorName);
        }

        public static List<string[]> GetMissingQuizzesByAuthorName(string authorName, string[] dBIdsOnDevice)
        {
            string queryString = $"{authorName}";
            foreach (string dbId in dBIdsOnDevice)
                queryString = queryString + "/" + dbId;
            return (List<string[]>)SendStringData(queryString, ServerRequestTypes.GetMissingQuizzesByAuthorName);
        }

        public static List<string[]> GetUsers(string username, int chunk)
        {
            return (List<string[]>)SendStringData($"{username}/{chunk}/-", ServerRequestTypes.GetUsers);
        }

        public static List<string[]> GetQuizzesByCategory(string category, int chunk)
        {
            return (List<string[]>)SendStringData($"{category}/{chunk}/-", ServerRequestTypes.GetQuizzesByCategory);
        }

        public static List<string[]> GetQuizzesByQuizName(string quizName, int chunk)
        {
            return (List<string[]>)SendStringData($"{quizName}/{chunk}/-", ServerRequestTypes.GetQuizzesByQuizName);
        }
        
        public static int GetNumberOfQuizzesByAuthorName(string username)
        {
            string returnData = (string)SendStringData($"{username}/-", ServerRequestTypes.GetNumberOfQuizzesByAuthorName);
            if (!int.TryParse(returnData, out int result))
            {
                result = 0;
            }
            return result;
        }

        public static async Task<OperationReturnMessage> SubscribeToQuiz(string dbId)
        {
            return (OperationReturnMessage)SendStringData($"{CredentialManager.Username}/{await SecureStorage.GetAsync("password")}/{dbId}/-", 
                ServerRequestTypes.SubscribeToQuiz);
        }

        public static async Task<OperationReturnMessage> UnsubscribeToQuiz(string dbId)
        {
            return (OperationReturnMessage)SendStringData($"{CredentialManager.Username}/{await SecureStorage.GetAsync("password")}/{dbId}/-", 
                ServerRequestTypes.UnsubscribeToQuiz);
        }

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

        public static bool GetQuiz(string dBId, string quizName, string authorName, string category)
        {
            string quizPath = App.UserPath + "/" + category + "/" + $"{quizName}`{authorName}/";

            Directory.CreateDirectory(quizPath);
            byte[] realmFile = (byte[])SendStringData($"{dBId}/-", ServerRequestTypes.GetRealmFile);
            string realmFilePath = quizPath + "/" + quizName + realmFileExtension;
            if (realmFile.Length > 0)
                File.WriteAllBytes(realmFilePath, realmFile);
            else
                return false;

            Realm realmDB = Realm.GetInstance(new RealmConfiguration(realmFilePath));
            IQueryable<Question> questionsWithPictures = realmDB.All<Question>().Where(question => question.NeedsPicture);
            foreach (Question question in questionsWithPictures)
            {
                byte[] jpegFile = (byte[])SendStringData($"{question.QuestionId}/{dBId}/-", ServerRequestTypes.GetJPEGImage);
                string jpegFilePath = quizPath + "/" + question.QuestionId + jpegFileExtension;
                if (jpegFile.Length > 0)
                    File.WriteAllBytes(jpegFilePath, jpegFile);
                else
                    return false;
            }
            QuizInfo infoCopy = new QuizInfo(QuizRosterDatabase.GetQuizInfo(dBId));
            infoCopy.SyncStatus = 2;
            QuizRosterDatabase.EditQuizInfo(infoCopy);

            return true;
        }
        
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

        #region Header Generators
        private static byte[] GenerateHeaderData(ServerRequestTypes type, uint size)
        {
            byte[] headerData = new byte[stringHeaderSize];
            headerData[0] = (byte)type;
            byte[] dataSize = BitConverter.GetBytes(size);
            dataSize.CopyTo(headerData, 1);
            return headerData;
        }

        private async static Task<byte[]> GenerateRealmHeader()
        {
            byte[] headerData = new byte[realmHeaderSize];

            byte[] username = Encoding.Unicode.GetBytes(CredentialManager.Username);
            Array.Copy(username, headerData, username.Length);

            byte[] password = Encoding.Unicode.GetBytes(await SecureStorage.GetAsync("password"));
            Array.Copy(password, 0, headerData, maxUsernameSize, password.Length);
            return headerData;
        }

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
        private static OperationReturnMessage ReceiveFromServerORM(byte[] toSend)
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                byte[] data = ServerConnector.SendByteArray(toSend);
                if (data.Length > 0)
                {
                    OperationReturnMessage message = (OperationReturnMessage)(data[0]);
                    ServerConnector.CloseConn();
                    return message;
                }
                else
                {
                    ServerConnector.CloseConn();
                    return OperationReturnMessage.FalseFailedConnection;
                }
            }
            else
            {
                return OperationReturnMessage.FalseNoConnection;
            }
        }

        private static string ReceiveFromServerStringData(byte[] toSend)
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                byte[] data = ServerConnector.SendByteArray(toSend);
                if (data.Length > 0)
                {
                    string dataString = Encoding.Unicode.GetString(data);
                    ServerConnector.CloseConn();
                    dataString = dataString.Trim();
                    return dataString;
                }
                else
                {
                    ServerConnector.CloseConn();
                    return "";
                }
            }
            else
            {
                return "";
            }
        }

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
                    ServerConnector.CloseConn();
                    return binaryFormatter.Deserialize(memStream) as List<string[]>;
                }
                else
                {
                    ServerConnector.CloseConn();
                    return new List<string[]>(0);
                }
            }
            else
            {
                return new List<string[]>(0);
            }
        }

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
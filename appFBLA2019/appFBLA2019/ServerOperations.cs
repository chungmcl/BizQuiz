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

        public static OperationReturnMessage DeleteLevel(string DBId)
        {
            return (OperationReturnMessage)SendStringData($"{CredentialManager.Username}/{CredentialManager.Password}/{DBId}", ServerRequestTypes.DeleteLevel);
        }

        public static List<string[]> GetLevelsByAuthorName(string authorName, int chunk)
        {
            return (List<string[]>)SendStringData($"{authorName}/{chunk}/-", ServerRequestTypes.GetLevelsByAuthorName);
        }

        public static List<string[]> GetUsers(string username, int chunk)
        {
            return (List<string[]>)SendStringData($"{username}/{chunk}/-", ServerRequestTypes.GetUsers);
        }

        public static List<string[]> GetLevelsByCategory(string category, int chunk)
        {
            return (List<string[]>)SendStringData($"{category}/{chunk}/-", ServerRequestTypes.GetLevelsByCategory);
        }

        public static List<string[]> GetLevelsByLevelName(string levelName, int chunk)
        {
            return (List<string[]>)SendStringData($"{levelName}/{chunk}/-", ServerRequestTypes.GetLevelsByLevelName);
        }
        
        public static int GetNumberOfLevelsByAuthorName(string username)
        {
            return int.Parse((string)SendStringData($"{username}/-", ServerRequestTypes.GetNumberOfLevelsByAuthorName));
        }

        public static bool SendLevel(string relativeLevelPath)
        {
            try
            {
                string realmFilePath = Directory.GetFiles(App.UserPath + relativeLevelPath, "*.realm").First();
                Realm realm = Realm.GetInstance(new RealmConfiguration(realmFilePath));
                LevelInfo info = realm.All<LevelInfo>().First();
                
                if (SendRealmFile(realmFilePath) != OperationReturnMessage.True)
                {
                    throw new Exception();
                }

                string[] imageFilePaths = Directory.GetFiles(App.UserPath + relativeLevelPath, "*.jpg");
                for (int i = 0; i < imageFilePaths.Length; i++)
                {
                    //[0] = path, [1] = fileName, [2] = dBId
                    string fileName = imageFilePaths[i].Split('/').Last().Split('.').First();
                    string dbID = info.DBId;
                    OperationReturnMessage message = SendImageFile(imageFilePaths[i], fileName, dbID);
                    
                    if (message == OperationReturnMessage.False)
                    {
                        throw new Exception();
                    }
                }

                // When finished, confirm with server that level send has completed
                OperationReturnMessage finalizationMessage = (OperationReturnMessage)SendStringData(
                    $"{info.DBId}`{info.LastModifiedDate}`{imageFilePaths.Length + 1}`" +
                    $"{CredentialManager.Username}`{CredentialManager.Password}`-",
                    ServerRequestTypes.FinalizeLevelSend);

                if (finalizationMessage == OperationReturnMessage.True)
                {
                    LevelInfo infoCopy = new LevelInfo(info);
                    infoCopy.SyncStatus = 2;
                    LevelRosterDatabase.EditLevelInfo(infoCopy);
                    return true;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                // Alert server that level send failed Delete records
                return false;
            }
        }

        public static bool GetLevel(string dBId, string levelName, string authorName)
        {
            string levelPath = App.UserPath + "/" + $"{levelName}`{authorName}/";

            Directory.CreateDirectory(App.UserPath + "/" + $"{levelName}`{authorName}");
            byte[] realmFile = (byte[])SendStringData($"{dBId}/-", ServerRequestTypes.GetRealmFile);
            string realmFilePath = levelPath + "/" + levelName + realmFileExtension;
            if (realmFile.Length > 0)
                File.WriteAllBytes(realmFilePath, realmFile);
            else
                return false;

            Realm realmDB = Realm.GetInstance(new RealmConfiguration(realmFilePath));
            IQueryable<Question> questionsWithPictures = realmDB.All<Question>().Where(question => question.NeedsPicture);
            foreach (Question question in questionsWithPictures)
            {
                byte[] jpegFile = (byte[])SendStringData($"{question.QuestionId}/{dBId}/-", ServerRequestTypes.GetJPEGImage);
                string jpegFilePath = levelPath + "/" + question.QuestionId + "/" + jpegFileExtension;
                if (jpegFile.Length > 0)
                    File.WriteAllBytes(jpegFilePath, jpegFile);
                else
                    return false;
            }

            return true;
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

        private static byte[] GenerateRealmHeader()
        {
            byte[] headerData = new byte[realmHeaderSize];

            byte[] username = Encoding.Unicode.GetBytes(CredentialManager.Username);
            Array.Copy(username, headerData, username.Length);

            byte[] password = Encoding.Unicode.GetBytes(CredentialManager.Password);
            Array.Copy(password, 0, headerData, maxUsernameSize, password.Length);
            return headerData;
        }

        private static byte[] GenerateImageHeader(string fileName, string dBId)
        {
            byte[] headerData = new byte[imageHeaderSize];

            byte[] username = Encoding.Unicode.GetBytes(CredentialManager.Username);
            Array.Copy(username, headerData, username.Length);

            byte[] password = Encoding.Unicode.GetBytes(CredentialManager.Password);
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

        #region Data Sends
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
                case ServerRequestTypes.GetNumberOfLevelsByAuthorName:
                    return ReceiveFromServerStringData(toSend);

                case ServerRequestTypes.GetLevelsByAuthorName:
                case ServerRequestTypes.GetLevelsByLevelName:
                case ServerRequestTypes.GetLevelsByCategory:
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
        private static OperationReturnMessage SendRealmFile(string path)
        {
            byte[] realmBytes = File.ReadAllBytes(path);
            byte[] realmHeader = GenerateRealmHeader();
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
        private static OperationReturnMessage SendImageFile(string path, string fileName, string dbId)
        {
            byte[] imageBytes = File.ReadAllBytes(path);
            byte[] imageHeader = GenerateImageHeader(fileName, dbId);
            byte[] header = GenerateHeaderData(ServerRequestTypes.AddJPEGImage, (uint)imageBytes.Length + (uint)imageHeader.Length);

            byte[] toSend = new byte[header.Length + imageHeader.Length + imageBytes.Length];
            header.CopyTo(toSend, 0);
            imageHeader.CopyTo(toSend, header.Length);
            imageBytes.CopyTo(toSend, header.Length + imageHeader.Length);
            return ReceiveFromServerORM(toSend);
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
            
            switch(ReceiveFromServerORM(toSend))
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
        FalseNoConnection,
        FalseFailedConnection
    }

    public enum ServerRequestTypes : byte
    {
        // "Command" Requests
        StringData,

        AddJPEGImage,
        AddRealmFile,
        FinalizeLevelSend,

        DeleteLevel,
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
        GetLevelsByAuthorName,
        GetLevelsByLevelName,
        GetLevelsByCategory,
        GetUsers,
        GetNumberOfLevelsByAuthorName
    }
}
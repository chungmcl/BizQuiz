using Plugin.Connectivity;
using Realms;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            SendStringData($"{username}/{password}/-", ServerRequestTypes.LoginAccount);
            return ReceiveFromServerORM();
        }

        public static OperationReturnMessage RegisterAccount(string username, string password, string email)
        {
            SendStringData($"{username}/{password}/{email}/-", ServerRequestTypes.RegisterAccount);
            return ReceiveFromServerORM();
        }

        public static OperationReturnMessage ChangePassword(string username, string currentPassword, string newPassword)
        {
            SendStringData($"{username}/{currentPassword}/{newPassword}/-", ServerRequestTypes.ChangePassword);
            return ReceiveFromServerORM();
        }

        public static OperationReturnMessage ConfirmEmail(string username, string confirmationToken)
        {
            SendStringData($"{username}/{confirmationToken}/-", ServerRequestTypes.ConfirmEmail);
            return ReceiveFromServerORM();
        }

        public static OperationReturnMessage ChangeEmail(string username, string password, string newEmail)
        {
            SendStringData($"{username}/{password}/{newEmail}/-", ServerRequestTypes.ChangeEmail);
            return ReceiveFromServerORM();
        }

        public static OperationReturnMessage DeleteAccount(string username, string password)
        {
            SendStringData($"{username}/{password}/-", ServerRequestTypes.DeleteAccount);
            return ReceiveFromServerORM();
        }

        public static string GetLastModifiedDate(string DBId)
        {
            SendStringData($"{DBId}/-", ServerRequestTypes.GetLastModifiedDate);
            return ReceiveFromServerStringData();
        }

        public static string GetEmail(string username, string password)
        {
            SendStringData($"{username}/{password}/-", ServerRequestTypes.GetEmail);
            return ReceiveFromServerStringData();
        }

        public static OperationReturnMessage DeleteLevel(string DBId)
        {
            SendStringData($"{CredentialManager.Username}/{CredentialManager.Password}/{DBId}", ServerRequestTypes.DeleteLevel);
            return ReceiveFromServerORM();
        }

        public static bool SendLevel(string relativeLevelPath)
        {
            try
            {
                string realmFilePath = Directory.GetFiles(App.Path + relativeLevelPath, "*.realm").First();
                Realm realm = Realm.GetInstance(new RealmConfiguration(realmFilePath));
                LevelInfo info = realm.All<LevelInfo>().First();

                string[] imageFilePaths = Directory.GetFiles(App.Path + relativeLevelPath, "*.jpg");
                SendRealmFile(realmFilePath);
                if (ReceiveFromServerORM() != OperationReturnMessage.True)
                    throw new Exception();

                for (int i = 0; i < imageFilePaths.Length; i++)
                {
                    //[0] = path, [1] = fileName, [2] = dBId
                    string fileName = imageFilePaths[i].Split('/').Last().Split('.').First();
                    string dbID = info.DBId;
                    SendImageFile(imageFilePaths[i], imageFilePaths[i].Split('/').Last().Split('.').First(), dbID);

                    OperationReturnMessage message = ReceiveFromServerORM();
                    if (message == OperationReturnMessage.False)
                        return false;
                }

                // When finished, confirm with server that level send has completed
                return true;
            }
            catch
            {
                // Alert server that level send failed
                // Delete records 
                return false;
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


        #region Receive Data from Server
        public static OperationReturnMessage ReceiveFromServerORM()
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                byte[] returnedBytes = ServerConnector.ReadByteArray(headerSize);
                if (!(returnedBytes == null || returnedBytes.Length < 5))
                {
                    int size = BitConverter.ToInt32(returnedBytes, 1);
                    OperationReturnMessage message = (OperationReturnMessage)(ServerConnector.ReadByteArray(size)[0]);
                    ServerConnector.CloseConn();
                    return message;
                }
                else
                    return OperationReturnMessage.FalseFailedConnection;
            }
            else
                return OperationReturnMessage.FalseNoConnection;
        }

        public static string ReceiveFromServerStringData()
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                byte[] returnedBytes = ServerConnector.ReadByteArray(headerSize);
                if (!(returnedBytes == null && returnedBytes.Length < 5))
                {
                    int size = BitConverter.ToInt32(returnedBytes, 1);
                    string data = Encoding.Unicode.GetString(ServerConnector.ReadByteArray(size));
                    ServerConnector.CloseConn();
                    data = data.Trim();
                    return data;
                }
                else
                    return null;
            }
            else
                return null;
        }
        #endregion


        #region Data send helper methods
        /// <summary>
        /// Send a string (Unicode) based data request or send to the server.
        /// </summary>
        /// <param name="data">The string data in Unicode to send.</param>
        /// <param name="dataType">The type of request to send to the server.</param>
        private static void SendStringData(string data, ServerRequestTypes dataType)
        {
            string dataAsString = data;
            byte[] dataAsBytes = Encoding.Unicode.GetBytes(dataAsString);
            byte[] headerData = GenerateHeaderData(dataType, (uint)dataAsBytes.Length);
            byte[] toSend = new byte[headerData.Length + dataAsBytes.Length];
            headerData.CopyTo(toSend, 0);
            dataAsBytes.CopyTo(toSend, headerData.Length);
            ServerConnector.SendByteArray(toSend);
        }

        /// <summary>
        /// Send realm file to the server.
        /// </summary>
        /// <param name="path">Path to the realm file on local device.</param>
        private static void SendRealmFile(string path)
        {
            byte[] realmBytes = File.ReadAllBytes(path);
            byte[] realmHeader = GenerateRealmHeader();
            byte[] header = GenerateHeaderData(ServerRequestTypes.AddRealmFile, (uint)realmBytes.Length + (uint)realmHeader.Length);

            byte[] toSend = new byte[header.Length + realmHeader.Length + realmBytes.Length];
            header.CopyTo(toSend, 0);
            realmHeader.CopyTo(toSend, header.Length);
            realmBytes.CopyTo(toSend, header.Length + realmHeader.Length);
            ServerConnector.SendByteArray(toSend);
        }

        /// <summary>
        /// Send JPEG image to server given the local path, filename, and DBId the image is related to
        /// </summary>
        /// <param name="path">Path to JPEG image file on local device.</param>
        /// <param name="fileName">Name of the JPEG image file.</param>
        /// <param name="dbId">DBId of the database image is contained in.</param>
        private static void SendImageFile(string path, string fileName, string dbId)
        {
            byte[] imageBytes = File.ReadAllBytes(path);
            byte[] imageHeader = GenerateImageHeader(dbId, dbId);
            byte[] header = GenerateHeaderData(ServerRequestTypes.AddJPEGImage, (uint)imageBytes.Length + (uint)imageHeader.Length);

            byte[] toSend = new byte[header.Length + imageHeader.Length + imageBytes.Length];
            header.CopyTo(toSend, 0);
            imageHeader.CopyTo(toSend, header.Length);
            imageBytes.CopyTo(toSend, header.Length + imageHeader.Length);
            ServerConnector.SendByteArray(toSend);
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
        AddLevelRecord,

        DeleteLevel,
        LoginAccount,
        RegisterAccount,
        DeleteAccount,
        ConfirmEmail,
        ChangeEmail,
        ChangePassword,

        // "Get" Requests
        GetEmail,
        GetJPEGImage,
        GetRealmFile,
        GetLastModifiedDate,
        GetLevelsByAuthorName
    }
}

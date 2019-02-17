using Plugin.Connectivity;
using Realms;
using System;
using System.Collections.Generic;
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

        public static OperationReturnMessage ReceiveFromServerORM()
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                int size = BitConverter.ToInt32(ServerConnector.ReadByteArray(headerSize), 1);
                OperationReturnMessage message = (OperationReturnMessage)(ServerConnector.ReadByteArray(size)[0]);
                return message;
            }
            else
            {
                return OperationReturnMessage.FalseNoConnection;
            }
        }

        public static string ReceiveFromServerStringData()
        {
            int size = BitConverter.ToInt32(ServerConnector.ReadByteArray(headerSize), 1);
            string data = Encoding.Unicode.GetString(ServerConnector.ReadByteArray(size));
            data = data.Trim();
            return data;
        }

        // Encrypts connection using SSL.
        /// <summary>
        /// Send a request or data to the server.
        /// </summary>
        /// <param name="dataType">
        /// The type of request/data to be sent
        /// </param>
        /// <param name="data">
        /// The data/string query to send
        /// </param>
        /// <returns>
        /// If the data successfully sent or not
        /// </returns>
        public static bool SendData(ServerRequestTypes dataType, object data)
        {
            if (ServerConnector.SetupConnection())
            {
                switch (dataType)
                {
                    case (ServerRequestTypes.AddJPEGImage):
                        SendImageFile((string[])data);
                        break;

                    case (ServerRequestTypes.AddRealmFile):
                        SendRealmFile((string)data);
                        break;

                    case (ServerRequestTypes.LoginAccount):
                    case (ServerRequestTypes.RegisterAccount):
                    case (ServerRequestTypes.ChangePassword):
                    case (ServerRequestTypes.GetEmail):
                    case (ServerRequestTypes.ConfirmEmail):
                    case (ServerRequestTypes.ChangeEmail):
                    case (ServerRequestTypes.DeleteAccount):
                    case (ServerRequestTypes.DeleteLevel):

                    case (ServerRequestTypes.GetLastModifiedDate):

                    case (ServerRequestTypes.StringData):
                        SendStringData((string)data, dataType);
                        return true;

                    case (ServerRequestTypes.GetJPEGImage):

                        break;

                    case (ServerRequestTypes.GetRealmFile):

                        break;
                }
                return false;
            }
            else
            {
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
        /// Send realm file to the server given the path to the realm file on current device.
        /// </summary>
        /// <param name="data"></param>
        private static void SendRealmFile(string data)
        {
            byte[] realmBytes = File.ReadAllBytes(data);
            byte[] realmHeader = GenerateRealmHeader();
            byte[] header = GenerateHeaderData(ServerRequestTypes.AddRealmFile, (uint)realmBytes.Length + (uint)realmHeader.Length);

            byte[] toSend = new byte[header.Length + realmHeader.Length + realmBytes.Length];
            header.CopyTo(toSend, 0);
            realmHeader.CopyTo(toSend, header.Length);
            realmBytes.CopyTo(toSend, header.Length + realmHeader.Length);
            ServerConnector.SendByteArray(toSend);
        }

        
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
                    ServerConnector.SendData(ServerRequestTypes.AddJPEGImage,
                        new string[] { imageFilePaths[i], imageFilePaths[i].Split('/').Last().Split('.').First(), dbID });

                    OperationReturnMessage message = ServerConnector.ReceiveFromServerORM();
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
    }



    public enum OperationReturnMessage : byte
    {
        True,
        False,
        TrueConfirmEmail,
        FalseInvalidCredentials,
        FalseInvalidEmail,
        FalseUsernameAlreadyExists,
        FalseNoConnection
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
        GetLastModifiedDate
    }
}

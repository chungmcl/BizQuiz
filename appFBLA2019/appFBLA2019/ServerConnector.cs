//BizQuiz App 2019

using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace appFBLA2019
{
    public static class ServerConnector
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
        private const int maxFileSize = 2147483591;
        // Server Release Build: 7777 Server Debug Build: 7778
        public static int Port { get { return 7778; } }

        public static string Server { get; set; }
        public static TcpClient client;
        public static NetworkStream netStream;

        // Raw-data stream of connection.
        public static SslStream ssl;


        public static bool ValidateCert(object sender, X509Certificate certificate,
              X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Allow untrusted certificates.
        }

        public static OperationReturnMessage ReceiveFromServerORM()
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                int size = BitConverter.ToInt32(ReadByteArray(headerSize), 1);
                return (OperationReturnMessage)(ReadByteArray(size)[0]);
            }
            else
            {
                return OperationReturnMessage.FalseNoConnection;
            }
        }

        public static string ReceiveFromServerStringData()
        {
            if (ssl == null)
                return "";

            int size = BitConverter.ToInt32(ReadByteArray(headerSize), 1);
            string data = Encoding.Unicode.GetString(ReadByteArray(size));
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
            if (SetupConnection())
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

        // Max byte size for arrays in C#, slightly under int.MaxValue - Can't process anything over this
        private static void CloseConn() // Close connection.
        {
            ssl.Close();
            netStream.Close();
            client.Close();
        }

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

        private static void SendStringData(string data, ServerRequestTypes dataType)
        {
            string dataAsString = data;
            byte[] dataAsBytes = Encoding.Unicode.GetBytes(dataAsString);
            byte[] headerData = GenerateHeaderData(dataType, (uint)dataAsBytes.Length);
            byte[] toSend = new byte[headerData.Length + dataAsBytes.Length];
            headerData.CopyTo(toSend, 0);
            dataAsBytes.CopyTo(toSend, headerData.Length);
            SendByteArray(toSend);
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
            SendByteArray(toSend);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">[0] = path, [1] = fileName, [2] = dBId</param>
        private static void SendImageFile(string[] data)
        {
            byte[] imageBytes = File.ReadAllBytes(data[0]);
            byte[] imageHeader = GenerateImageHeader(data[1], data[2]);
            byte[] header = GenerateHeaderData(ServerRequestTypes.AddJPEGImage, (uint)imageBytes.Length + (uint)imageHeader.Length);

            byte[] toSend = new byte[header.Length + imageHeader.Length + imageBytes.Length];
            header.CopyTo(toSend, 0);
            imageHeader.CopyTo(toSend, header.Length);
            imageBytes.CopyTo(toSend, header.Length + imageHeader.Length);
            SendByteArray(toSend);
        }
        
        private static void SendByteArray(byte[] data)
        {
            lock (ssl)
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    ssl.Write(data, 0, data.Length);
                    ssl.Flush();
                }
            }
        }

        private static byte[] ReadByteArray(int size)
        {
            if (ssl != null)
            {
                lock (ssl)
                {
                    if (CrossConnectivity.Current.IsConnected)
                    {
                        byte[] buffer = new byte[1024];

                        List<byte> data = new List<byte>();
                        int bytes = -1;
                        int bytesRead = 0;
                        do
                        {
                            int toRead = 0;
                            if ((size - bytesRead) > buffer.Length)
                            {
                                toRead = buffer.Length;
                            }
                            else
                            {
                                toRead = size - bytesRead;
                            }

                            bytes = ssl.Read(buffer, 0, toRead);
                            bytesRead += bytes;

                            if (bytes > 0)
                            {
                                for (int i = 0; i < bytes; i++)
                                {
                                    data.Add(buffer[i]);
                                }
                            }
                        } while (data.Count < size);
                        data.RemoveRange(size, data.Count - size);
                        return data.ToArray();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
                return null;
        }

        private static bool SetupConnection()
        {
            try
            {
                if (CrossConnectivity.Current.IsConnected)
                {
                    client = new TcpClient();
                    lock (client)
                    {
                        client = new TcpClient(AddressFamily.InterNetworkV6);
                        client.Client.DualMode = true;
                        var result = client.BeginConnect(Server, Port, null, null);
                        var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(7));
                        if (!success)
                        {
                            throw new Exception("Failed to connect - Timeout exception.");
                        }

                        netStream = client.GetStream();

                        lock (netStream)
                        {
                            ssl = new SslStream(netStream, false,
                            new RemoteCertificateValidationCallback(ValidateCert));

                            lock (ssl)
                            {
                                // UNCOMMENT DURING RELEASE
                                //ssl.WriteTimeout = 5000;
                                //ssl.ReadTimeout = 10000;

                                ssl.AuthenticateAsClient("BizQuizServer");
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
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
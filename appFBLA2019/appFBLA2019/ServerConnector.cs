//BizQuiz App 2019

using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace appFBLA2019
{
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
        LoginAccount,
        RegisterAccount,
        DeleteAccount,
        ConfirmEmail,
        ChangeEmail,

        // "Get" Requests
        GetEmail,

        GetJPEGImage,
        GetRealmFile,

        // Returns
        SavedJPEGImage,

        SavedRealmFile,
        GotEmail,
        True,
        False,
        TrueConfirmEmail
    }

    public static class ServerConnector
    {
        // Server Release Build: 7777 Server Debug Build: 7778
        public static int Port { get { return 7777; } }

        public static string Server { get; set; }
        public static TcpClient client;
        public static NetworkStream netStream;

        // Raw-data stream of connection.
        public static SslStream ssl;

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

                        break;

                    case (ServerRequestTypes.AddRealmFile):

                        break;

                    case (ServerRequestTypes.LoginAccount):
                    case (ServerRequestTypes.RegisterAccount):
                    case (ServerRequestTypes.GetEmail):
                    case (ServerRequestTypes.ConfirmEmail):
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

        public static bool ValidateCert(object sender, X509Certificate certificate,
              X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Allow untrusted certificates.
        }

        private const int headerSize = 5;
        private const int maxFileSize = 2147483591;

        // Max byte size for arrays in C#, slightly under int.MaxValue - Can't process anything over this
        private static void CloseConn() // Close connection.
        {
            ssl.Close();
            netStream.Close();
            client.Close();
        }

        private static byte[] GenerateHeaderData(ServerRequestTypes type, uint size)
        {
            byte[] headerData = new byte[5];
            headerData[0] = (byte)type;
            byte[] dataSize = BitConverter.GetBytes(size);
            dataSize.CopyTo(headerData, 1);
            return headerData;
        }

        private static byte[] ReadByteArray(int size)
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
                        toRead = buffer.Length - (size - bytesRead);
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

        private static void SendByteArray(byte[] data)
        {
            if (CrossConnectivity.Current.IsConnected)
            {
                ssl.Write(data, 0, data.Length);
                ssl.Flush();
            }
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
                                ssl.WriteTimeout = 5000;
                                ssl.ReadTimeout = 10000;

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
}
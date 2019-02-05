using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace appFBLA2019
{
    public static class ServerConnector
    {
        private const int maxFileSize = 2147483591; // Max byte size for arrays in C#, slightly under int.MaxValue - Can't process anything over this
        private const int headerSize = 5;

        public static TcpClient client;
        public static NetworkStream netStream;  // Raw-data stream of connection.
        public static SslStream ssl;            // Encrypts connection using SSL.

        public static string Server { get; set; }
        public static int Port { get { return 7777; } }

        public static bool SendData(ServerRequestTypes dataType, object data)
        {
            SetupConnection();

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

        public static string ReceiveFromServerStringData()
        {
            int size = BitConverter.ToInt32(ReadByteArray(headerSize), 1);
            string data = Encoding.Unicode.GetString(ReadByteArray(size));
            data = data.Trim();
            return data;
        }

        public static OperationReturnMessage ReceiveFromServerORM()
        {
            int size = BitConverter.ToInt32(ReadByteArray(headerSize), 1);
            return (OperationReturnMessage)(ReadByteArray(size)[0]);
        }

        private static void SendByteArray(byte[] data)
        {
            ssl.Write(data, 0, data.Length);
            ssl.Flush();
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
            byte[] buffer = new byte[1024];

            List<byte> data = new List<byte>();
            int bytes = -1;
            int bytesRead = 0;
            do
            {

                bytes = ssl.Read(buffer, 0, size - bytesRead);
                bytesRead += bytes;
                if (bytes > 0)
                    for (int i = 0; i < bytes; i++)
                        data.Add(buffer[i]);

            } while (data.Count < size);
            data.RemoveRange(size, data.Count - size);
            return data.ToArray();
        }

        private static void SetupConnection()
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

            ssl = new SslStream(netStream, false,
                new RemoteCertificateValidationCallback(ValidateCert));

            ssl.WriteTimeout = 5000;
            
            ssl.AuthenticateAsClient("BizQuizServer");
        }

        private static void CloseConn() // Close connection.
        {
            ssl.Close();
            netStream.Close();
            client.Close();
        }

        public static bool ValidateCert(object sender, X509Certificate certificate,
              X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Allow untrusted certificates.
        }
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

    public enum OperationReturnMessage : byte
    {
        True,
        False,
        TrueConfirmEmail
    }
}


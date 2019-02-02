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

        public static void SendData(ServerRequestTypes dataType, object data)
        {
            switch (dataType)
            {
                case (ServerRequestTypes.AddJPEGImage):
                    break;
                case (ServerRequestTypes.AddRealmFile):
                    break;
                case (ServerRequestTypes.LoginAccount):
                    break;
                case (ServerRequestTypes.RegisterAccount):
                    break;
                case (ServerRequestTypes.StringData):
                    break;
                case (ServerRequestTypes.GetJPEGImage):
                    break;
                case (ServerRequestTypes.GetRealmFile):
                    break;
                case (ServerRequestTypes.GetEmail):
                    break;

            }

        }

        private static void SendByteArray(byte[] data)
        {
            ssl.Write(data, 0, data.Length);
            ssl.Flush();
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
    
    public enum ServerRequestTypes
    {
        // "Command" Requests
        StringData = 0b00000001,
        AddJPEGImage = 0b00000010,
        AddRealmFile = 0b00000011,
        LoginAccount = 0b00000100,
        RegisterAccount = 0b00000101,
        DeleteAccount = 0b00000110,
        ConfirmEmail = 0b00000111,

        // "Get" Requests
        GetEmail = 0b00001001,
        GetJPEGImage = 0b00001010,
        GetRealmFile = 0b00001011
    }
}


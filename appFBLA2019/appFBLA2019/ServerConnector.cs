using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace appFBLA2019
{
    public static class ServerConnector
    {
        private static TcpClient tcp;
        private static NetworkStream nwStream;
        private const string serverIP = "50.54.142.236";
        private const int serverPort = 7777;
        private const int timeoutTimeMilliseconds = 5000;
        private const double timeoutTimeSeconds = 5;

        /// <summary>
        /// Sends request to server database to add/create new account
        /// </summary>
        /// <param name="username">Username to be added</param>
        /// <param name="password">Password to be added</param>
        /// <returns>True if connected and sent, false if could not connect</returns>
        public static async Task<bool> QueryDB(string dbQuery)
        {
            tcp = new TcpClient();
            IAsyncResult result = tcp.BeginConnect(serverIP, serverPort, null, null);

            bool connectionSuccess = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(timeoutTimeSeconds));

            if (!connectionSuccess)
            {
                return false;
            }
            else
            {
                byte[] sendBuffer = Encoding.ASCII.GetBytes(dbQuery);

                nwStream = tcp.GetStream();

                nwStream.WriteTimeout = timeoutTimeMilliseconds;
                nwStream.Write(sendBuffer, 0, sendBuffer.Length);

                return true;
            }
        }

        /// <summary>
        /// Receive data/message from database proceeding sending request
        /// </summary>
        /// <returns>The data/message from the server</returns>
        public async static Task<string> ReceiveFromDB()
        {

            byte[] bytesToRead = new byte[tcp.ReceiveBufferSize];
            nwStream.ReadTimeout = timeoutTimeMilliseconds;
            int bytesRead = nwStream.Read(bytesToRead, 0, tcp.ReceiveBufferSize);
            string databaseMessage = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);

            tcp.Client.Disconnect(true);
            tcp.Client.Close();
            tcp.Close();

            return databaseMessage;
        }
    }
}

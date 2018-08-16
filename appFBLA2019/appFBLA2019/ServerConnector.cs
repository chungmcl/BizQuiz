using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace appFBLA2019
{
    public static class ServerConnector
    {
        private static TcpClient tcp;
        private static NetworkStream nwStream;
        private const string serverIP = "50.54.142.236";
        private const int serverPort = 7777;
        /// <summary>
        /// Sends request to server database to add/create new account
        /// </summary>
        /// <param name="username">Username to be added</param>
        /// <param name="password">Password to be added</param>
        public static async Task QueryDB(string dbQuery)
        {
            tcp = new TcpClient(serverIP, serverPort);

            byte[] sendBuffer = Encoding.ASCII.GetBytes(dbQuery);

            nwStream = tcp.GetStream();

            nwStream.Write(sendBuffer, 0, sendBuffer.Length);
        }

        /// <summary>
        /// Receive data/message from database proceeding sending request
        /// </summary>
        /// <returns>The data/message from the server</returns>
        public async static Task<string> ReceiveFromDB()
        {

            byte[] bytesToRead = new byte[tcp.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, tcp.ReceiveBufferSize);
            string databaseMessage = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);

            tcp.Client.Disconnect(true);
            tcp.Client.Close();
            tcp.Close();

            return databaseMessage;
        }
    }
}

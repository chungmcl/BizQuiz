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
        // The byte length of the header in transmissions
        private const int headerSize = 5;

        // Server Release Build: 7777 Server Debug Build: 7778
        public static int Port { get { return 7778; } }

        // The address of the server
        public static string Server { get; set; }

        // The TcpClient on to of the TLS (SSL) stream
        public static TcpClient client;
        public static NetworkStream netStream;

        /// <summary>
        /// An empty to object to serve as a thread lock
        /// </summary>
        private static object lockObj = new object();

        // Raw-data stream of connection encrypted with TLS.
        public static SslStream ssl;
        
        // Delegate for SslStream to validate certificate
        public static bool ValidateCert(object sender, X509Certificate certificate,
              X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        
        /// <summary>
        /// Send byte data to server and read what the server returns
        /// </summary>
        /// <param name="data">The byte data to send to server.</param>
        /// <returns>A byte array of what the server returns</returns>
        public static byte[] SendByteArray(byte[] data)
        {
            // Prevent sending and receiving corrupted data
            // because of thread collisions through the lock
            lock (lockObj)
            {
                if (SetupConnection())
                {
                    try
                    {
                        if (ssl != null)
                        {
                            lock (ssl)
                            {

                                if (CrossConnectivity.Current.IsConnected)
                                {
                                    ssl.Write(data, 0, data.Length);
                                    ssl.Flush();

                                    byte[] header = ReadByteArray(headerSize);
                                    int size = BitConverter.ToInt32(header, 1);
                                    if (header.Length >= 5)
                                    {
                                        byte[] returnedData = ReadByteArray(size);
                                        return returnedData;
                                    }
                                    else
                                        return new byte[0];
                                }
                                else
                                    return new byte[0];
                            }
                        }
                        else
                            return new byte[0];
                    }
                    catch
                    {
                        return new byte[0];
                    }
                }
                else
                {
                    return new byte[0];
                }
            }
        }

        /// <summary>
        /// Read byte array of what the server returns.
        /// </summary>
        /// <param name="size">Size of the file to read</param>
        /// <returns>Byte array of data that the server returns</returns>
        private static byte[] ReadByteArray(int size)
        {
            try
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
                            toRead = buffer.Length;
                        else
                            toRead = size - bytesRead;

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
                    return new byte[0];
                }
            }
            catch (Exception ex)
            {
                BugReportHandler.SaveReport(ex, "ServerConnector.ReadByteArray()");
                return new byte[0];
            }
        }

        /// <summary>
        /// Setup connection between client and server
        /// </summary>
        /// <returns></returns>
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
                        var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
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
                                ssl.ReadTimeout = 5000;

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
        
        /// <summary>
        /// Close and dispose of network objects.
        /// </summary>
        public static void CloseConn() // Close connection.
        {
            if (ssl != null)
                ssl.Close();
            if (netStream != null)
                netStream.Close();
            if (client != null)
                client.Close();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace Client
{
    public class Connection
    {
        IPEndPoint endpoint;
        string username;
        private string token;

        private bool run = true;
        private TcpClient client;
        private Thread cThread;

        public Connection(IPEndPoint endPoint, string username) {
            this.endpoint = endPoint;
            this.username = username;
        }

        public bool Start()
        {
            if (username == null) {
                return false;
            }
            run = true;
            client = new TcpClient();
            client.NoDelay = true;
            client.SendTimeout = 5000;
            client.ReceiveTimeout = 120000;
            client.ReceiveBufferSize = 8192; // Random for now
            client.ExclusiveAddressUse = false;
            client.LingerState = new LingerOption(true, 2);
            try
            {
                if (!client.ConnectAsync(endpoint).Wait(10000))
                {
                    return false;
                }
                NetworkStream loginStream = client.GetStream();
                byte[] usernameBytes = Encoding.Unicode.GetBytes(username);
                byte[] loginBuffer = new byte[2 + usernameBytes.Length];
                loginBuffer[0] = (byte)1;
                loginBuffer[1] = (byte)usernameBytes.Length;
                usernameBytes.CopyTo(loginBuffer, 2);
                if (!loginStream.WriteAsync(loginBuffer, 0, loginBuffer.Length).Wait(30000))
                {
                    client.Close();
                    return false;
                }
                byte[] buff = new byte[1];
                loginStream.ReadExactly(buff, 0, 1);

            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
                return false;
            }
            catch (AggregateException e)
            {
                MessageBox.Show(e.Message, "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch (IOException e)
            {
                MessageBox.Show(e.Message, "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public void Stop()
        {
            run = false;
            client.Close();
            client.Dispose();
        }

        public void connectionHandler()
        {
            NetworkStream s = client.GetStream();
            try
            {
                while (run)
                {
                    // Base Logic
                }
            } finally
            {
                s.Close();
            }
        }
    }
}

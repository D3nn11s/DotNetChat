using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
            if (username != null) {
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
                client.Connect(endpoint);
                NetworkStream loginStream = client.GetStream();
                byte[] usernameBytes = Encoding.Unicode.GetBytes(username);
                byte[] loginBuffer = new byte[2 + usernameBytes.Length];
                loginBuffer[0] = (byte)1;
                loginBuffer[1] = (byte)loginBuffer.Length;
                loginBuffer.CopyTo(loginBuffer, 2);
                loginStream.Write(loginBuffer, 0, loginBuffer.Length);
                // Expect Token or Denial
                loginStream.Close();
            }
            catch (SocketException e)
            {
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

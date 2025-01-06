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

        private void startCThread(NetworkStream stream)
        {
            cThread = new Thread(new ParameterizedThreadStart(connectionHandler));
            cThread.Start(stream);
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
            client.ReceiveTimeout = 10000;
            client.ReceiveBufferSize = 8192; // Random for now
            client.ExclusiveAddressUse = false;
            client.LingerState = new LingerOption(true, 2);
            try
            {
                // First: Connect
                if (!client.ConnectAsync(endpoint).Wait(10000))
                {
                    return false;
                }
                NetworkStream stream = client.GetStream();

                // Now send our Login Attempt
                byte[] usernameBytes = Encoding.Unicode.GetBytes(username);
                byte[] loginBuffer = new byte[2 + usernameBytes.Length];
                loginBuffer[0] = (byte)1;
                loginBuffer[1] = (byte)usernameBytes.Length;
                usernameBytes.CopyTo(loginBuffer, 2);
                if (!stream.WriteAsync(loginBuffer, 0, loginBuffer.Length).Wait(30000))
                {
                    // Could not send bytes :(
                    client.Close();
                    return false;
                }

                // Wait for the Tokenlength or Error Packet ID
                byte[] buf = new byte[1];
                stream.ReadExactly(buf, 0, 1);
                int size = buf[0];
                if (size < 2)
                {
                    // Do Error Message Handeling here
                    // TODO Get Type of Error like "Username Taken"
                    return false;
                }

                // Recieve Token
                buf = new byte[size];
                stream.ReadExactly(buf, 0, size);
                token = Encoding.Unicode.GetString(buf);
                MessageBox.Show(token);
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

            // All good now! We should be connected at this point.
            // Let's start this Client Thread and allow Packets to be parsed.
            // startCThread(stream)

            return true;
        }

        public void Stop()
        {
            run = false;
            client.Close();
            client.Dispose();
        }

        private void connectionHandler(object streamObj)
        {
            NetworkStream s = (NetworkStream)streamObj;
            try
            {
                while (run)
                {
                    // Base Logic
                    break; // Temporary Breakout
                }
            } finally
            {
                s.Close();
            }
        }
    }
}

using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 5063);
            listener.Start();

            Console.WriteLine("Listening...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Client connected!");

                Thread Read = new Thread(ReadStream);
                Read.Start(client);
            }
        }

        static void ReadStream(object obj) 
        {
            // giving the object through this way because of threads
            TcpClient client = (TcpClient)obj;  

            NetworkStream stream = client.GetStream();

            User thisUser = null;

            bool connected = true;

            try
            {
                while (connected)
                {
                    int packetID = stream.ReadByte();

                    switch (packetID)
                    {
                        case -1:
                            connected = false;
                            break;
                        case 1:
                            Console.WriteLine("login");

                            // Length of username read from the clients first byte
                            int length = stream.ReadByte();
                            byte[] buffer = new byte[length];
                            // Reads the username
                            stream.ReadExactly(buffer, 0, length);

                            string content = Encoding.Unicode.GetString(buffer);
                            Console.WriteLine("Name: " + content);


                            string token = Token.GenerateToken(); // ToDo: Include saving this to the user, object oriented, and stuff like that
                            byte[] tokenbytes = Encoding.Unicode.GetBytes(token);
                            byte[] sendBytes = new byte[tokenbytes.Length + 1];
                            sendBytes[0] = Convert.ToByte(tokenbytes.Length);
                            tokenbytes.CopyTo(sendBytes, 1);

                            stream.Write(sendBytes, 0, sendBytes.Length);



                            break;
                        case 2:
                            Console.WriteLine("Disconnect");
                            break;
                        case 3:
                            Console.WriteLine("MSG");
                            break;
                        case 4:
                            Console.WriteLine("PM");
                            break;
                        case 5:
                            Console.WriteLine("SYNC");
                            break;
                        case 6:
                            Console.WriteLine("RECONNECT");
                            break;
                    }
                }
            } catch (IOException e)
            {
                Console.WriteLine("Client Error " + e);
            }
            client.Close();
            Console.WriteLine("Client Disconnected.");
        }
    }
}

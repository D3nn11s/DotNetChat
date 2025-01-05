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
            TcpClient client = (TcpClient)obj;  // horor 

            NetworkStream stream = client.GetStream();

            bool connected = true;

            while (connected) {
                int packetID = stream.ReadByte();

                switch (packetID)
                {
                    case -1:
                        connected = false;
                        break;
                    case 1:
                        Console.WriteLine("login");
                        int length = stream.ReadByte();
                        byte[] buffer = new byte[length];
                        stream.ReadExactly(buffer, 0, length);

                        string content = Encoding.Unicode.GetString(buffer);
                        Console.WriteLine("Name: " + content);


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
            client.Close();
            Console.WriteLine("Client Disconnected.");
        }
    }
}

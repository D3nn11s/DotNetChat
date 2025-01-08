using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;

namespace Server
{
    internal class Program
    {

        static HashSet<User> Users = new HashSet<User>();


        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.IPv6Any, 5063);
            listener.ExclusiveAddressUse = false;
            listener.Server.DualMode = true;
            listener.Server.SendTimeout = 30000;
            listener.Server.ReceiveTimeout = 30000;
            listener.Server.ReceiveBufferSize = 32768;
            listener.Server.SendBufferSize = 32768;
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

        static void sendErrorID(ref NetworkStream stream, byte errorID)
        {
            byte[] errorBytes = { 1, errorID };
            stream.Write(errorBytes, 0, errorBytes.Length);
        }

        static void broadcastMessageToAll(ChatMessage message) 
            //  Fixed (maybe), still take a look at this
        {
            Console.WriteLine("sent");
            byte[] userBytes = Encoding.Unicode.GetBytes(message.Username);
            byte[] messageBytes = Encoding.Unicode.GetBytes(message.Message);

            

            UInt16 messageBytesLength = Convert.ToUInt16(messageBytes.Length);

            var lengthMessagebytes = BitConverter.GetBytes(messageBytesLength);

            byte[] messageBuffer = new byte[2 + messageBytes.Length + userBytes.Length + lengthMessagebytes.Length];

            messageBuffer[0] = (byte)2;
            messageBuffer[1] = (byte)userBytes.Length;

            // copy username into messagepacket
            userBytes.CopyTo(messageBuffer, 2); // 2 + length of userbytes

            // copy length of message into packet
            lengthMessagebytes.CopyTo(messageBuffer, userBytes.Length + 2);

            messageBytes.CopyTo(messageBuffer, userBytes.Length + lengthMessagebytes.Length + 2);

            foreach (var user in Users) {
            
            user.stream.Write(messageBuffer, 0, messageBytes.Length);
            }
        }

        static void ReadStream(object obj) 
        {
            // giving the object through this way because of threads
            TcpClient client = (TcpClient)obj;  

            NetworkStream stream = client.GetStream();

            User? thisUser = null;

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
                            Console.WriteLine("Login");

                            // Length of username read from the clients first byte
                            int length = stream.ReadByte();
                            byte[] buffer = new byte[length];
                            // Reads the username
                            stream.ReadExactly(buffer, 0, length);

                            string username = Encoding.Unicode.GetString(buffer);
                            Console.WriteLine("Username: " + username);

                            // Check if Username exists,
                            // if null, then it doesn't exist yet. Allow this user to temporarily own it.
                            User? exist = Users.FirstOrDefault(user => user.username.Equals(username), null);
                            if (exist != null)
                            {
                                sendErrorID(ref stream, 1);
                                break;
                            }

                            Token token = Token.createToken();
                            byte[] tokenbytes = Encoding.Unicode.GetBytes(token.GetToken());
                            byte[] sendBytes = new byte[tokenbytes.Length + 1];
                            sendBytes[0] = Convert.ToByte(tokenbytes.Length);
                            tokenbytes.CopyTo(sendBytes, 1);
                            stream.Write(sendBytes, 0, sendBytes.Length);

                            thisUser = new User(username, token, stream);
                            Users.Add(thisUser);
                            break;
                        case 2:
                            Console.WriteLine("Disconnect");
                            if (thisUser != null)
                            {
                                Users.Remove(thisUser);
                                thisUser = null;
                            }
                            break;
                        case 3:
                            Console.WriteLine("MSG");
                            int messageLength = stream.ReadByte();
                            byte[] messagebuffer = new byte[messageLength];

                            stream.ReadExactly(messagebuffer, 0, messageLength);
                            string message = Encoding.Unicode.GetString(messagebuffer);

                            broadcastMessageToAll(new ChatMessage(thisUser.username, message));
                            break;
                        case 4:
                            Console.WriteLine("PM");
                            break;
                        case 5:
                            Console.WriteLine("SYNC");
                            break;
                        case 6:
                            Console.WriteLine("Reconnect");

                            // Receive Token String
                            int reconTokenLength = stream.ReadByte();
                            byte[] reconTokenBytes = new byte[reconTokenLength];
                            stream.ReadExactly(reconTokenBytes, 0, reconTokenLength);
                            string reconToken = Encoding.Unicode.GetString(reconTokenBytes);

                            // Check if Token exists
                            User? user = Users.FirstOrDefault(user => user.token.GetToken().Equals(reconToken));
                            if (user == null)
                            {
                                sendErrorID(ref stream, 2);
                                break;
                            }
                            stream.WriteByte(6);
                            thisUser = user;
                            break;
                    }
                }
            } catch (IOException e)
            {
                Console.WriteLine("Client Error " + e.Message);
            }
            client.Close();
            client.Dispose();
            Console.WriteLine("Client Disconnected.");
        }
    }
}

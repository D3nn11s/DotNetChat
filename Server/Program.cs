using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;

namespace Server
{
    internal class Program
    {

        static HashSet<User> Users = new HashSet<User>();
        static LinkedList<Message> Messages = new LinkedList<Message>();


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

        static void broadcastMessageToAll(Message message) 
        {
            byte[] userBytes = Encoding.Unicode.GetBytes(message.getUsername());
            byte[] messageBytes = Encoding.Unicode.GetBytes(message.getContent());

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
            
            user.stream.Write(messageBuffer, 0, messageBuffer.Length);
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
                            // Length of username read from the clients first byte
                            int length = stream.ReadByte();
                            byte[] buffer = new byte[length];
                            // Reads the username
                            stream.ReadExactly(buffer, 0, length);

                            string username = Encoding.Unicode.GetString(buffer);

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
                            Console.WriteLine("User logged in: " + thisUser);
                            break;
                        case 2:
                            Console.WriteLine("Disconnect Packet: " + thisUser);
                            connected = false;
                            break;
                        case 3:
                            if (thisUser == null)
                            {
                                sendErrorID(ref stream, 3);
                                break;
                            }
                            int messageLength1 = stream.ReadByte();
                            int messageLength2 = stream.ReadByte();
                            UInt16 result = (UInt16)((messageLength2 << 8) | messageLength1); // little endian format
                            byte[] messagebuffer = new byte[result];

                            stream.ReadExactly(messagebuffer, 0, result);
                            string message = Encoding.Unicode.GetString(messagebuffer);
                            ChatMessage msg = new ChatMessage(thisUser, message);
                            Messages.AddLast(msg);
                            broadcastMessageToAll(msg);
                            break;
                        case 4:
                            // PM
                            if (thisUser == null)
                            {
                                sendErrorID(ref stream, 3);
                                break;
                            }
                            int usernameLength = stream.ReadByte();
                            byte[] usernameBytes = new byte[usernameLength];
                            stream.ReadExactly(usernameBytes, 0, usernameLength);
                            int pmMessageLength1 = stream.ReadByte();
                            int pmMessageLength2 = stream.ReadByte();
                            UInt16 pmMessagelength = (UInt16)((pmMessageLength2 << 8) | pmMessageLength1);
                            byte[] messageBytes = new byte[pmMessagelength];
                            stream.ReadExactly(messageBytes, 0, pmMessagelength);
                            string PMUsername = Encoding.Unicode.GetString(usernameBytes);
                            string PMMessage = Encoding.Unicode.GetString(messageBytes);
                            User? pmTargetUser = Users.FirstOrDefault(user => user.username.Equals(PMUsername), null);
                            if (pmTargetUser != null)
                            {
                                PrivateMessage pm = new PrivateMessage(thisUser, pmTargetUser, PMMessage);
                                Messages.AddLast(pm);
                                Console.WriteLine("PM: " + thisUser + " -> " + PMUsername + ": " + PMMessage);
                                pmTargetUser.sendPrivateMsg(pm);
                            }
                            break;
                        case 5:
                            Console.WriteLine("SYNC");
                            int amountMessages = Messages.Count;
                            List<byte> syncMessageBuffer = new List<byte>();
                            syncMessageBuffer.Add(5);
                            syncMessageBuffer.Add((byte)amountMessages);

                            foreach (Message m in Messages)
                            {
                                byte[] usersBytes = Encoding.Unicode.GetBytes(m.getUsername());
                                byte[] messagesBytes = Encoding.Unicode.GetBytes(m.getContent());

                                byte usersLength = (byte)usersBytes.Length;
                                UInt16 messagesLength = (UInt16)messagesBytes.Length;
                                byte messagesLength1 = (byte)(messagesLength & 0xFF);
                                byte messagesLength2 = (byte)((messagesLength >> 8) & 0xFF);

                                if (m is PrivateMessage pm)
                                {
                                    if (pm.Target.username != thisUser.username)
                                    {
                                        continue;
                                    }
                                    byte[] targetUserBytes = Encoding.Unicode.GetBytes(pm.Target.username);
                                    byte targetUserLength = (byte)targetUserBytes.Length;

                                    syncMessageBuffer.Add(4); // PM identifier
                                    syncMessageBuffer.Add(usersLength);
                                    syncMessageBuffer.AddRange(usersBytes);
                                    syncMessageBuffer.Add(targetUserLength);
                                    syncMessageBuffer.AddRange(targetUserBytes);
                                    syncMessageBuffer.Add(messagesLength1);
                                    syncMessageBuffer.Add(messagesLength2);
                                    syncMessageBuffer.AddRange(messagesBytes);
                                }
                                else
                                {
                                    syncMessageBuffer.Add(2); // MSG identifier
                                    syncMessageBuffer.Add(usersLength);
                                    syncMessageBuffer.AddRange(usersBytes);
                                    syncMessageBuffer.Add(messagesLength1);
                                    syncMessageBuffer.Add(messagesLength2);
                                    syncMessageBuffer.AddRange(messagesBytes);
                                }
                            }

                            byte[] syncMessageArray = syncMessageBuffer.ToArray();
                            stream.Write(syncMessageArray, 0, syncMessageArray.Length);
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
                            user.token.resetValidity();
                            break;
                        case 7:
                            // heartbeat packet response to client
                            stream.WriteByte(7);
                            break;
                    }
                }
            } catch (IOException e)
            {
                Console.WriteLine("Client Error " + e.Message + " @ User: " + thisUser);
            }
            Console.WriteLine("Client Disconnected: " + thisUser);
            client.Close();
            client.Dispose();
            if (thisUser != null)
            {
                Users.Remove(thisUser);
                thisUser = null;
            }
        }
    }
}

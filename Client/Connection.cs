using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    public class Connection
    {
        IPEndPoint endpoint;
        string username;
        private string token;

        private volatile bool run = true;
        private TcpClient client;
        private Thread cThread;

        private NetworkStream stream;

        public Connection(IPEndPoint endPoint, string username) {
            this.endpoint = endPoint;
            this.username = username;
        }

        private void startCThread()
        {
            cThread = new Thread(new ThreadStart(connectionHandler));
            cThread.Start();
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
                stream = client.GetStream();

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
                    int errorID = stream.ReadByte();
                    string error = Utils.GetErrorMessage("NET" + Convert.ToString(errorID));
                    MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                // Recieve Token
                buf = new byte[size];
                stream.ReadExactly(buf, 0, size);
                token = Encoding.Unicode.GetString(buf);

                // All good now! We should be connected at this point.
                // Let's start this Client Thread and allow Packets to be parsed.
                startCThread();
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

        public void sendPacket(byte[] buffer)
        {
            if (stream != null)
            {
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        private void connectionHandler()
        {
            sendPacket(new byte[] { 5 });
            bool ExpectHeartbeat = false;
            try
            {
                while (run)
                {
                    int PacketID;
                    try
                    {
                        PacketID = stream.ReadByte();
                    }
                    catch (System.IO.IOException)
                    {
                        if (ExpectHeartbeat)
                        {
                            break;
                        }
                        else
                        {
                            stream.WriteByte(7);
                            ExpectHeartbeat = true;
                            continue;
                        }
                    }
                    switch (PacketID)
                    {
                        case -1:
                            run = false;
                            break;
                        case 1:
                            // ERROR : 1 (byte PacketType, byte FehlerID)
                            int errorID = stream.ReadByte();
                            string error = Utils.GetErrorMessage("NET"+Convert.ToString(errorID));
                            MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                        case 2:
                            //  MSG : 2 (byte Länge, string username, byte längeMessage, string message)
                            int lengthUser = stream.ReadByte();
                            byte[] buffer = new byte[lengthUser];

                            stream.ReadExactly(buffer, 0, lengthUser);

                            string username = Encoding.Unicode.GetString(buffer);

                            int lengthMessage1 = stream.ReadByte();
                            int lengthMessage2 = stream.ReadByte();

                            UInt16 result = (UInt16)((lengthMessage2 << 8) | lengthMessage1);
                            buffer = new byte[result];

                            stream.ReadExactly(buffer, 0, result);

                            string message = Encoding.Unicode.GetString(buffer);

                            Application.Current.Dispatcher.Invoke(() => Global.ChatMessages.Add(new ChatMessage(username, message)));
                            break;

                        case 3:
                            // PM : 3 (byte Länge, string vonUsername, byte längeMessage, string message)
                            int lengthPMUser = stream.ReadByte();
                            byte[] pmUsernameBytes = new byte[lengthPMUser];

                            stream.ReadExactly(pmUsernameBytes, 0, lengthPMUser);

                            string pmUsername = Encoding.Unicode.GetString(pmUsernameBytes);

                            int pmLengthMessage1 = stream.ReadByte();
                            int pmLengthMessage2 = stream.ReadByte();

                            UInt16 pmLengthMessageLength = (UInt16)((pmLengthMessage2 << 8) | pmLengthMessage1);
                            buffer = new byte[pmLengthMessageLength];

                            stream.ReadExactly(buffer, 0, pmLengthMessageLength);

                            string pmMessage = Encoding.Unicode.GetString(buffer);

                            Application.Current.Dispatcher.Invoke(() => Global.ChatMessages.Add(new ChatMessage(pmUsername, pmMessage, new SolidColorBrush(Color.FromRgb(234, 207, 255)))));
                            break;
                        //case 4:
                        //    // DISCONNECT : 4 (byte CauseID)
                        //    break;
                        case 5:
                            // SYNCRESPONSE: 5 (byte anzahlnachrichten(* MSG : 2 (byte Länge, string username, byte längeMessage, string message))
                            // Read the number of messages
                            int amountMessages = stream.ReadByte();

                            for (int i = 0; i < amountMessages; i++)
                            {
                                int msgIdentifier = stream.ReadByte();
                                if (msgIdentifier == 3)
                                {
                                    break;
                                }

                                int userLength = stream.ReadByte();
                                byte[] userBytes = new byte[userLength];
                                stream.ReadExactly(userBytes, 0, userLength);
                                string username1 = Encoding.Unicode.GetString(userBytes);

                                if (msgIdentifier == 4) // PM identifier
                                {
                                    int targetUserLength = stream.ReadByte();
                                    byte[] targetUserBytes = new byte[targetUserLength];
                                    stream.ReadExactly(targetUserBytes, 0, targetUserLength);
                                    string targetUsername = Encoding.Unicode.GetString(targetUserBytes);

                                    // Read the message length (UInt16)
                                    int messageLength1 = stream.ReadByte();
                                    int messageLength2 = stream.ReadByte();
                                    UInt16 messageLength = (UInt16)((messageLength2 << 8) | messageLength1);
                                    byte[] messageBytes = new byte[messageLength];
                                    stream.ReadExactly(messageBytes, 0, messageLength);
                                    string message1 = Encoding.Unicode.GetString(messageBytes);
                                    Application.Current.Dispatcher.Invoke(() => Global.ChatMessages.Insert(0, new ChatMessage(username1, message1, new SolidColorBrush(Color.FromRgb(234, 207, 255)))));
                                }
                                else if (msgIdentifier == 2) // Normal message identifier
                                {
                                    // Read the message length (UInt16)
                                    int messageLength1 = stream.ReadByte();
                                    int messageLength2 = stream.ReadByte();
                                    UInt16 messageLength = (UInt16)((messageLength2 << 8) | messageLength1);
                                    byte[] messageBytes = new byte[messageLength];
                                    stream.ReadExactly(messageBytes, 0, messageLength);
                                    string message1 = Encoding.Unicode.GetString(messageBytes);

                                    Application.Current.Dispatcher.Invoke(() => Global.ChatMessages.Insert(0, new ChatMessage(username1, message1)));
                                }
                            }
                            break;
                        case 6:
                            // Success
                            break;
                        case 7:
                            // server heartbeat answer
                            ExpectHeartbeat = false;
                            break;

                    }

                }
            } catch (IOException) {
            } catch (ObjectDisposedException) {
            } finally
            {
                stream.Close();
                stream = null;
                Global.Reset();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow mw = new MainWindow();
                    mw.Show();
                    Application.Current.Windows[0].Close();
                });
            }
        }
    }
}

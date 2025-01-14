using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Client
{
    /// <summary>
    /// Interaktionslogik für ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {

        public ChatWindow()
        {
            InitializeComponent();

            this.Title += Global.username;

            Global.ChatMessages = new ObservableCollection<ChatMessage>();
            ChatListBox.ItemsSource = Global.ChatMessages;
            Global.ChatMessages.CollectionChanged += this.scrollOnMessage;
        }

        private void scrollOnMessage(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.NewItems != null)
            {
                ScrollViewer? v = GetScrollViewer(ChatListBox);
                
                if (v != null && (v.ScrollableHeight - v.VerticalOffset) == 0)
                {
                    int lastIndex = args.NewItems.Count - 1;
                    ChatListBox.ScrollIntoView(args.NewItems[lastIndex]);
                }
            }
        }

        private ScrollViewer? GetScrollViewer(DependencyObject parent)
        {
            int c = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < c; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is ScrollViewer tChild)
                {
                    return tChild;
                }

                ScrollViewer? childOfChild = GetScrollViewer(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            sendMessage();
        }

        private void sendMessage()
        {
            string message = MessageTextBox.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                if (message.StartsWith("/pm "))
                {
                    message = message.Substring(4);
                    int index = message.IndexOf(' ');
                    if (index == -1)
                    {
                        return;
                    }
                    string targetUsername = message.Substring(0, index);
                    message = message.Substring(index + 1);

                    if (message.Length < 1 || targetUsername.Length < 1)
                    {
                        return;
                    }

                    // ChatListBox.Dispatcher.Invoke(() => Global.ChatMessages.Add(new ChatMessage("you", message)));
                    byte[] messageBytes = Encoding.Unicode.GetBytes(message);
                    UInt16 messageBytesLength = Convert.ToUInt16(messageBytes.Length);
                    var lengthMessagebytes = BitConverter.GetBytes(messageBytesLength); // 2 bytes
                    byte[] userBytes = Encoding.Unicode.GetBytes(targetUsername);

                    byte[] messageBuffer = new byte[2 + userBytes.Length + messageBytes.Length + lengthMessagebytes.Length];

                    messageBuffer[0] = (byte)4; // packetid
                    messageBuffer[1] = (byte)userBytes.Length;

                    userBytes.CopyTo(messageBuffer, 2);

                    // copy length of message into packet
                    lengthMessagebytes.CopyTo(messageBuffer, 2 + userBytes.Length);

                    messageBytes.CopyTo(messageBuffer, lengthMessagebytes.Length + 2 + userBytes.Length);

                    Global.connection.sendPacket(messageBuffer);
                }
                else
                {

                    // ChatListBox.Dispatcher.Invoke(() => Global.ChatMessages.Add(new ChatMessage("you", message)));
                    byte[] messageBytes = Encoding.Unicode.GetBytes(message);
                    UInt16 messageBytesLength = Convert.ToUInt16(messageBytes.Length);
                    var lengthMessagebytes = BitConverter.GetBytes(messageBytesLength); // 2 bytes

                    byte[] messageBuffer = new byte[1 + messageBytes.Length + lengthMessagebytes.Length];

                    messageBuffer[0] = (byte)3; // packetid

                    // copy length of message into packet
                    lengthMessagebytes.CopyTo(messageBuffer, 1);

                    messageBytes.CopyTo(messageBuffer, lengthMessagebytes.Length + 1);

                    Global.connection.sendPacket(messageBuffer);
                }

                MessageTextBox.Text = "";
                MessageTextBox.Focus();
            }
        }

        private void disconnect_Button(object sender, RoutedEventArgs e)
        {
            Global.Reset();
            MainWindow mw = new MainWindow();
            mw.Show();
            this.Close();
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                sendMessage();
            }
        }
    }

   
}

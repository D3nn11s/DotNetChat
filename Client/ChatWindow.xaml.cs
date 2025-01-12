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


            Global.ChatMessages = new ObservableCollection<ChatMessage>();
            ChatListBox.ItemsSource = Global.ChatMessages;
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;

            if (!string.IsNullOrWhiteSpace(message))
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
                MessageTextBox.Text = "";
            }
        }
    }

   
}

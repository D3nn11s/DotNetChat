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

        public ObservableCollection<ChatMessage> ChatMessages { get; set; }
        public ChatWindow()
        {
            InitializeComponent();

            ChatMessages = new ObservableCollection<ChatMessage>();
            ChatListBox.ItemsSource = ChatMessages;
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;

            if (!string.IsNullOrWhiteSpace(message))
            {
                ChatMessages.Add(new ChatMessage { User = "You", Message = message });
                MessageTextBox.Text = "";
            }
        }
    }

    public class ChatMessage
    {
        public string User { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return User + ": " + Message;
        }
    }
}

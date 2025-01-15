using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string CleanUpUsername(string rawUsername)
        {
            return Regex.Replace(rawUsername, @"\s+", "");
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            connect();
        }

        private void Textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                connect();
            }
        }

        private void connect()
        {
            string username = CleanUpUsername(usernameTextbox.Text);
            if (username.Length < 1)
            {
                MessageBox.Show(Utils.GetErrorMessage("noUsername"), "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string ip = ipTextbox.Text.Trim();
            var (code, res) = Utils.ParseIP(ip);
            switch (code)
            {
                case 0:
                    Global.username = username;
                    Global.IPEndPoint = res;
                    Global.connection = new Connection(res, username);
                    if (!Global.connection.Start())
                    {
                        Global.Reset();
                        MessageBox.Show(string.Format(Utils.GetErrorMessage("failedToConnect"), res.ToString()), "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
                    ChatWindow cw = new ChatWindow();
                    cw.Show();
                    this.Close();
                    break;
                case 1:
                    MessageBox.Show(Utils.GetErrorMessage("ipParseError"), "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 2:
                    MessageBox.Show(Utils.GetErrorMessage("invalidHost"), "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 3:
                    MessageBox.Show(string.Format(Utils.GetErrorMessage("invalidPortNumber"), ip.Split(":")[1]), "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }

        private void usernameTextbox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox? tb = sender as TextBox;
            if (tb == null) return;
            int index = tb.CaretIndex;
            int length = tb.Text.Length;
            tb.Text = CleanUpUsername(usernameTextbox.Text);
            tb.CaretIndex = index - (length - tb.Text.Length);
        }
    }
}
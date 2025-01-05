using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            string username = usernameTextbox.Text.Trim();
            if (username.Length < 1)
            {
                MessageBox.Show(Utils.GetErrorMessage(this, "noUsername"), "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string ip = ipTextbox.Text.Trim();
            var (code, res) = Utils.ParseIP(ip);
            switch (code)
            {
                case 0:
                    Global.username = username;
                    Global.IPEndPoint = res;
                    MessageBox.Show("Connecting to " + res.Address.ToString() + ":" + res.Port + "! (If this worked)" , "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case 1:
                    MessageBox.Show(Utils.GetErrorMessage(this, "ipParseError"), "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 2:
                    MessageBox.Show(Utils.GetErrorMessage(this, "invalidHost"), "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 3:
                    MessageBox.Show(String.Format(Utils.GetErrorMessage(this, "invalidPortNumber"), ip.Split(":")[1]), "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }
    }
}
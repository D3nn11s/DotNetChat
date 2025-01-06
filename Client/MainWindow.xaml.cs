﻿using System.Windows;
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
                    Global.connection = new Connection(res, username);
                    if (!Global.connection.Start())
                    {
                        MessageBox.Show(string.Format(Utils.GetErrorMessage(this, "failedToConnect"), res.ToString()), "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    }
                    ChatWindow cw = new ChatWindow();
                    cw.Show();
                    this.Close();
                    break;
                case 1:
                    MessageBox.Show(Utils.GetErrorMessage(this, "ipParseError"), "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 2:
                    MessageBox.Show(Utils.GetErrorMessage(this, "invalidHost"), "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case 3:
                    MessageBox.Show(string.Format(Utils.GetErrorMessage(this, "invalidPortNumber"), ip.Split(":")[1]), "DotNetChat", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }
    }
}
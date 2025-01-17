﻿using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Controls;

namespace Client
{
    public class Global
    {
        public static string username;
        public static IPEndPoint IPEndPoint;
        public static Connection connection;

        public static ObservableCollection<ChatMessage> ChatMessages;

        public static void Reset()
        {
            if (connection != null)
            {
                try
                {
                    connection.sendPacket(new byte[] { 2 });
                } catch { }
                connection.Stop();
                connection = null;
                ChatMessages = null;
            }
        }
    }
}

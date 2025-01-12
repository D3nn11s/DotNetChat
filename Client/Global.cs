using System.Collections.ObjectModel;
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

    }
}

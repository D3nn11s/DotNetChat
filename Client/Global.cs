using System.Collections.ObjectModel;
using System.Net;

namespace Client
{
    public class Global
    {
        public static string username;
        public static IPEndPoint IPEndPoint;
        public static Connection connection;

        public static ObservableCollection<ChatMessage> ChatMessages { get; set; }
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Client
{
    public class ChatMessage
    {
        public ChatMessage(string user, string message)
        {
            this.User = user;
            this.Message = message;
        }
        public ChatMessage(string user, string message, SolidColorBrush msgColor)
        {
            this.User = user;
            this.Message = message;
            this.msgColor = msgColor;
        }

        public string User { get; set; }
        public string Message { get; set; }
        public SolidColorBrush msgColor { get; set; } = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

    }
}

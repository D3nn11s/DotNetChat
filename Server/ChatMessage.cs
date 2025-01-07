using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ChatMessage
    {
        public string Username { get; set; }
        public string Message { get; set; }
        
        public ChatMessage(string Username, string Message)
        {
            this.Username = Username;
            this.Message = Message;
        }
    }
}

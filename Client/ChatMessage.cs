using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ChatMessage
    {
        public ChatMessage(string user, string message) 
        {
        this.User = user;
        this.Message = message;
        }

        public string User { get; set; }
        public string Message { get; set; }

    }
}

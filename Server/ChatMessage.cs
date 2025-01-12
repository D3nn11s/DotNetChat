using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ChatMessage : Message
    {
        public User Sender;
        public ChatMessage(User Sender, string Message) : base(Message, Sender.username) {
            this.Sender = Sender;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PrivateMessage : Message
    {
        public User Target { get; }
        public User Sender { get; }

        public PrivateMessage(User Sender, User Target, string Message) : base(Message, Sender.username)
        {
            this.Target = Target;
            this.Sender = Sender;
        }
    }
}

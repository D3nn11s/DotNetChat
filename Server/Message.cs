using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    abstract public class Message
    {
        private string message;
        private string username;

        public Message(string message, string username)
        {
            this.message = message;
            this.username = username;
        }

        public string getContent()
        {
            return message;
        }

        public string getUsername()
        {
            return username;
        }

        public override string ToString()
        {
            return getUsername() + ": " + getContent();
        }
    }
}

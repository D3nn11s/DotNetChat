using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class User
    {
        public string username { get; }
        public Token token { get; }
        
        public NetworkStream stream { get; }

        public User(string username, Token token, NetworkStream stream)
        {
            this.username = username;
            this.token = token;
            this.stream = stream;
        }

        public override int GetHashCode()
        {
            return this.username.GetHashCode() + this.token.GetHashCode();
        }
    }
}

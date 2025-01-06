using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class User
    {
        public string username { get; }
        public Token token { get; }

        public User(string username, Token token)
        {
            this.username = username;
            this.token = token;
        }

        public override int GetHashCode()
        {
            return this.username.GetHashCode() + this.token.GetHashCode();
        }
    }
}

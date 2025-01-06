using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class User
    {
        string username;
        Token token;

        public User(string username, Token token)
        {
            this.username = username;
            this.token = token;
        }
    }
}

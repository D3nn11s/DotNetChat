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
        public bool loggedIn { get; set;  }

        public NetworkStream stream { get; set; }

        public User(string username, Token token, NetworkStream stream)
        {
            this.username = username;
            this.token = token;
            this.stream = stream;
            loggedIn = true;
        }

        public override int GetHashCode()
        {
            return this.username.GetHashCode() + this.token.GetHashCode();
        }

        public override string ToString()
        {
            return this.username;
        }

        public void sendPrivateMsg(Message message)
        {
            byte[] userBytes = Encoding.Unicode.GetBytes(message.getUsername());
            byte[] messageBytes = Encoding.Unicode.GetBytes(message.getContent());
            UInt16 messageBytesLength = Convert.ToUInt16(messageBytes.Length);
            var lengthMessagebytes = BitConverter.GetBytes(messageBytesLength);
            byte[] messageBuffer = new byte[2 + messageBytes.Length + userBytes.Length + lengthMessagebytes.Length];
            messageBuffer[0] = (byte)3;
            messageBuffer[1] = (byte)userBytes.Length;
            userBytes.CopyTo(messageBuffer, 2);
            lengthMessagebytes.CopyTo(messageBuffer, userBytes.Length + 2);
            messageBytes.CopyTo(messageBuffer, userBytes.Length + lengthMessagebytes.Length + 2);
            stream.Write(messageBuffer, 0, messageBuffer.Length);
        }
    }
}

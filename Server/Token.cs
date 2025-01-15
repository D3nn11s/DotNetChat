using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Token
    {
        static TimeSpan Lifetime = TimeSpan.FromHours(2);
        private static int TOKENLENGTH = 32;

        private string token;
        private DateTime validUntil;

        private Token(string token)
        {
            this.token = token;
            resetValidity();
        }

        public string GetToken() { return token; }

        public DateTime getValidDateTime() { return validUntil; }

        public void resetValidity()
        {
            validUntil = DateTime.Now.Add(Lifetime);
        }

        // Avoid using DateTime.Now because of the repeated Getter Function Usage.
        // I like performance.
        public bool isExpired(DateTime now)
        {
            return now > validUntil;
        }

        public static Token createToken()
        {
            return new Token(GenerateToken());
        }

        static public string GenerateToken()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder result = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < TOKENLENGTH; ++i)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

        public override int GetHashCode()
        {
            return this.token.GetHashCode() + this.validUntil.GetHashCode();
        }
    }
}

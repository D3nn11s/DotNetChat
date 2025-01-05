using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client
{
    class Utils
    {
        /// <summary>
        /// Nimmt aus dem aktuellen Fenster-Kontext (Culture) die Übersetzung für eine Error Nachricht heraus.
        /// <br></br>
        /// Gibt <b>null</b> zurück bei keinem Treffer.
        /// </summary>
        /// <param name="w">Fenster-Kontext</param>
        /// <param name="key">Schlüssel der Error Nachricht</param>
        /// <returns>Die übersetze Error Nachricht</returns>
        public static string? GetErrorMessage(Window w, string key)
        {
            try
            {
                return (string)((ResourceDictionary)w.FindResource("errors"))[key];
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static (int, IPEndPoint?) ParseIP(string ip)
        {
            if (ip.Length < 1)
            {
                return (1, null);
            }
            IPEndPoint endpoint = new IPEndPoint(0, 0);
            if (IPEndPoint.TryParse(ip, out endpoint))
            {
                if (endpoint.Port == 0)
                {
                    return (1, null);
                }
                return (0, endpoint);
            }
            string[] tokens = ip.Split(':');
            try
            {
                IPHostEntry host = Dns.GetHostEntry(tokens[0]);
                if (host.AddressList.Length < 1)
                {
                    return (2, null);
                }
                if (tokens.Length > 1)
                {
                    short port;
                    if (short.TryParse(tokens[1], out port) && port > 0)
                    {
                        return (0, new IPEndPoint(host.AddressList[0], port));
                    }
                    return (3, null);
                }
                return (0, new IPEndPoint(host.AddressList[0], 5063));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return (1, null);
        }
    }
}

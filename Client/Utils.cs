using System.Net;
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
        /// <param name="key">Schlüssel der Error Nachricht</param>
        /// <returns>Die übersetze Error Nachricht</returns>
        public static string? GetErrorMessage(string key)
        {
            try
            {
                var dictionary = (ResourceDictionary)Application.Current.Resources["errors"];

                if (dictionary.Contains(key))
                {
                    return dictionary[key].ToString();
                }
                else
                {
                    return null;
                }
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
                    return (0, new IPEndPoint(endpoint.Address, 5063));
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

using System.IO;
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


        private static string tempFile { get
            {
                return Path.Combine(Path.GetTempPath(), "dotnetchat");
            }
        }
        public static void storeSuccessfulConnectionDetails(string username, string ip, string token)
        {
            StreamWriter writer = File.CreateText(tempFile);
            writer.WriteLine(username);
            writer.WriteLine(ip);
            writer.WriteLine(token);
            writer.Close();
        }

        public static void clearConnectionDetails()
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }

        public static ConnectionDetails? getSuccessfulConnectionDetails()
        {
            try
            {
                string path = tempFile;
                if (File.Exists(path))
                {
                    StreamReader reader = File.OpenText(path);
                    try
                    {
                        string username = reader.ReadLine();
                        string ip = reader.ReadLine();
                        string token = reader.ReadLine();
                        if (username == null || username.Length < 1 || ip == null || ip.Length < 1 || token == null || token.Length < 1)
                        {
                            return null;
                        }
                        return new ConnectionDetails(username, ip, token);
                    }
                    finally
                    {
                        reader.Close();
                    }
            }
            } catch (Exception ex)
            {
                Console.WriteLine("Failed to read temp file -> " + ex.Message);
            }
            return null;
        }
        public class ConnectionDetails
        {
            public string username;
            public IPEndPoint ip;
            public string token;
            public ConnectionDetails(string username, string ip, string token)
            {
                this.username = username;
                this.ip = IPEndPoint.Parse(ip);
                this.token = token;
            }
        }
    }
}

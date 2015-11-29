using System.Collections.Generic;
using System.Linq;
using DotRas;

namespace wox.plugin.vpn
{
    public class ConnectionManager
    {
        public static readonly string PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User);

        public IEnumerable<Connection> EnumerateConnections()
        {
            var activeConnections = RasConnection.GetActiveConnections();

            using (var phoneBook = new RasPhoneBook())
            {
                phoneBook.Open(PhoneBookPath);

                foreach (var e in phoneBook.Entries)
                {
                    var connection = new Connection(e)
                    {
                        Name = e.Name,
                        Status = RasConnectionState.Disconnected
                    };

                    var activeConnection = activeConnections.FirstOrDefault(a => a.EntryName == e.Name);
                    if (activeConnection != null)
                    {
                        connection.SetConnected(activeConnection);
                        connection.Status
                            = activeConnection.GetConnectionStatus().ConnectionState;
                    }

                    yield return connection;
                }
            }
        }
    }
}

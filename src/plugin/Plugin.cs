using System;
using System.Collections.Generic;
using System.Linq;
using DotRas;
using Wox.Plugin;

namespace wox.plugins.vpn
{
    public class Plugin : IPlugin
    {
        private PluginInitContext _context;

        public IEnumerable<Result> EnumerateResults(Query query)
        {
            var phoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User);

            var activeConnections = RasConnection.GetActiveConnections();

            Func<RasEntry, bool> filter = e => true;

            if (!String.IsNullOrEmpty(query.FirstSearch))
                filter = e => e.Name.Contains(query.FirstSearch);

            using (var phoneBook = new RasPhoneBook())
            {
                phoneBook.Open(phoneBookPath);

                foreach (var e in phoneBook.Entries.Where(filter))
                {
                    var connection = activeConnections.FirstOrDefault(a => a.EntryName == e.Name);
                    if (connection != null)
                    {
                        yield return new Result(e.Name)
                        {
                            IcoPath = "Images\\disconnect.png",
                            SubTitle = "Disconnect from VPN",
                            ContextData = connection,
                            Action = context =>
                            {
                                _context.API.StartLoadingBar();
                                connection.HangUp();
                                _context.API.StopLoadingBar();
                                _context.API.ChangeQuery(String.Empty);
                                return true;
                            }
                        };
                    }
                    else
                    {
                        var credentials = e.GetCredentials();

                        yield return new Result(e.Name)
                        {
                            IcoPath = "Images\\connect.png",
                            SubTitle = "Connect to VPN",
                            Action = context =>
                            {
                                _context.API.StartLoadingBar();
                                _context.API.ChangeQueryText(e.Name);
                                var dialer = new RasDialer
                                {
                                    EntryName = e.Name,
                                    PhoneBookPath = phoneBookPath,
                                    Credentials = credentials
                                };

                                var connected = false;

                                dialer.StateChanged += (sender, args) =>
                                {
                                    _context.API.PushResults(query, _context.CurrentPluginMetadata, new List<Result>
                                    {
                                        new Result {Title = args.State.ToString()}
                                    });
                                };


                                dialer.DialCompleted += (sender, args) =>
                                {
                                    _context.API.PushResults(query, _context.CurrentPluginMetadata, new List<Result>
                                    {
                                        new Result {Title = "Connected: " + args.Connected.ToString()}
                                    });
                                    if (args.Connected)
                                        connected = true;
                                };

                                dialer.Dial();

                                _context.API.StopLoadingBar();

                                _context.API.ChangeQuery(String.Empty);
                                return connected;
                            }
                        };
                    }

                }
            }

        }

        public List<Result> Query(Query query)
        {
            return EnumerateResults(query).ToList();
        }

        public void Init(PluginInitContext context)
        {
            _context = context;
        }
    }
}

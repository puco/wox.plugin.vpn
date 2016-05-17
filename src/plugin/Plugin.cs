using System;
using System.Collections.Generic;
using System.Linq;
using DotRas;
using Wox.Plugin;

namespace wox.plugin.vpn
{
    public class Plugin : IPlugin
    {
        private PluginInitContext _context;

        private Result Transform(Connection connection)
        {
            var result = new Result
            {
                Title = connection.Name,
                IcoPath = "Images\\disconnect.png"
            };

            switch (connection.Status)
            {
                case RasConnectionState.Disconnected:
                    result.IcoPath = "Images\\connect.png";
                    result.SubTitle = "Connect to VPN";
                    result.Action = context =>
                    {
                        _context.API.StartLoadingBar();
                        connection.Connect();
                        _context.API.StopLoadingBar();
                        return true;
                    };
                    break;

                case RasConnectionState.Connected:
                    result.IcoPath = "Images\\disconnect.png";
                    result.SubTitle = "Disconnect from VPN";
                    result.Action = context =>
                    {
                        _context.API.StartLoadingBar();
                        connection.Disconnect();
                        _context.API.StopLoadingBar();
                        return true;
                    };
                    break;

                default:
                    result.SubTitle = "Status: " + connection.Status;
                    break;
            }
            return result;
        }

        private IEnumerable<Result> EnumerateResults(Query query)
        {
            Func<Connection, bool> filter = e => true;

            if (!string.IsNullOrEmpty(query.FirstSearch))
                filter = e => e.Name.Contains(query.FirstSearch);

            return ConnectionManager.EnumerateConnections()
                .Where(filter)
                .Select(Transform);
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

using System;
using System.Net;
using System.Security.Cryptography;
using DotRas;

namespace wox.plugin.vpn
{
    public class Connection
    {
        private readonly RasEntry _entry;

        public RasConnectionState Status { get; set; }

        public string Name { get; set; }

        private RasConnection _rasConnection;
        private readonly NetworkCredential _networkCredential;

        public event EventHandler StateChanged;

        public Connection(RasEntry entry, RasConnection rasConnection = null)
        {
            _entry = entry;
            _networkCredential = _entry.GetCredentials();
            SetConnected(rasConnection);
        }

        internal void SetConnected(RasConnection connection)
        {
            if (connection == null) return;

            _rasConnection = connection;
            Status = RasConnectionState.Connected;
        }

        public void Connect()
        {
            if (ConnectionManager.PhoneBookPath == null)
                throw new Exception("Phonebookpath is nukk");

            var dialer = new RasDialer
            {
                EntryName = Name,
                PhoneBookPath = ConnectionManager.PhoneBookPath,
                Credentials = _networkCredential
            };

            dialer.StateChanged += (sender, args) =>
            {
                Status = args.State;
                OnStateChanged();
            };

            dialer.Dial();
        }


        public void Disconnect()
        {
            _rasConnection.HangUp();
        }

        protected virtual void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
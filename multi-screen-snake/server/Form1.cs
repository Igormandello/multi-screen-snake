using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace server
{
    class ConnectedComputer
    {
        public int Number { get; set; }
        public IPAddress IP { get; set; }
        public TcpClient Client { get; set; }
        public Stream ClientStream { get; set; }

        public ConnectedComputer(int n, TcpClient c)
        {
            this.Number = n;
            this.IP = ((IPEndPoint)c.Client.RemoteEndPoint).Address;
            this.Client = c;
            this.ClientStream = c.GetStream();
        }
    }

    public partial class Form1 : Form
    {
        List<ConnectedComputer> clientList = new List<ConnectedComputer>();
        TcpListener server;

        bool started = false;

        public Form1()
        {
            InitializeComponent();

            server = new TcpListener(IPAddress.Any, 1337);
            server.Start();

            Task.Run(() => AcceptConnections());
        }

        private void AcceptConnections()
        {
            while (!started)
            {
                TcpClient client = null;

                try
                {
                    client = server.AcceptTcpClient();
                    ConnectedComputer newClient = new ConnectedComputer(-1, client);

                    int n = Convert.ToInt32(new StreamReader(newClient.ClientStream).ReadLine());
                    newClient.Number = n;

                    if (clientList.Find(c => c.IP == newClient.IP) != null)
                        continue;

                    clientList.Add(newClient);
                    Invoke(new Action(() => lbConnected.Items.Add("#" + n + " - " + Dns.GetHostEntry(newClient.IP).HostName)));
                }
                catch
                {
                    if (client != null)
                        client.Close();
                }
            }
        }

        private void StartClick(object sender, EventArgs e)
        {
            this.started = true;
            clientList.Sort(ClientCompare);
        }

        private int ClientCompare(ConnectedComputer c1, ConnectedComputer c2)
        {
            return c1.Number.CompareTo(c2.Number);
        }
    }
}

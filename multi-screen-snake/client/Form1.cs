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
    public partial class Form1 : Form
    {
        List<TcpClient> clientList = new List<TcpClient>();
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
                    IPAddress clientIP = ((IPEndPoint)client.Client.RemoteEndPoint).Address;

                    if (clientList.Find(c => ((IPEndPoint)client.Client.RemoteEndPoint).Address == clientIP) != null)
                        continue;

                    clientList.Add(client);
                    Invoke(new Action(() => lbConnected.Items.Add(Dns.GetHostEntry(clientIP).HostName)));
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
        }
    }
}

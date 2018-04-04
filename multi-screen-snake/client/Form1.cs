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

namespace client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCon_Click(object sender, EventArgs e)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(IPAddress.Parse("127.0.0.1"), 1337);

                StreamWriter writer = new StreamWriter(client.GetStream());
                writer.WriteLine(tbClient.Text);
                writer.Flush();
            }
            catch
            {
                MessageBox.Show("Something went wrong");
            }
        }
    }
}

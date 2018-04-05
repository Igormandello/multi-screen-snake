using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client
{
    public partial class Form1 : Form
    {
        bool connected = false;
        StreamReader reader;
        Graphics g;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnCon_Click(object sender, EventArgs e)
        {
            if (connected)
                return;

            try
            {
                TcpClient client = new TcpClient();
                client.Connect(IPAddress.Parse("177.220.18.65"), 1337);

                StreamWriter writer = new StreamWriter(client.GetStream());
                reader = new StreamReader(client.GetStream());

                writer.WriteLine(tbClient.Text);
                writer.Flush();

                InitializeCanvas();
            }
            catch
            {
                MessageBox.Show("Something went wrong");
            }
        }

        private void InitializeCanvas()
        {
            this.Controls.Remove(pnlConnect);
            connected = true;

            this.FormBorderStyle = FormBorderStyle.None;
            this.Bounds = new Rectangle(new Point(0, 0), Screen.PrimaryScreen.Bounds.Size);
            g = this.CreateGraphics();

            Task.Run(() => AcceptDraw());
        }

        private void AcceptDraw()
        {
            g.Clear(this.TransparencyKey);

            while (connected)
            {
                String message = reader.ReadLine();
                if (message == "CLEAR")
                {
                    g.Clear(this.TransparencyKey);
                    continue;
                }
                else
                    while (message != "END")
                    {
                        String[] props = message.Split(',');
                        Rectangle r = new Rectangle(Convert.ToInt32(props[0]), Convert.ToInt32(props[1]), Convert.ToInt32(props[2]), Convert.ToInt32(props[3]));

                        g.FillRectangle(Brushes.Black, r);

                        message = reader.ReadLine();
                    }
            }
        }
    }
}

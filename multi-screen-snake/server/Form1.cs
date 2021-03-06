﻿using System;
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

namespace server
{
    public partial class Form1 : Form
    {
        public static int ColCount { get; set; }
        public static int RowCount { get; set; }

        List<ConnectedComputer> clientList = new List<ConnectedComputer>();
        TcpListener server;

        bool started = false;
        Graphics g;
        Snake snake;

        yDirection yDir = yDirection.None;
        xDirection xDir = xDirection.Right;

        public Form1()
        {
            InitializeComponent();

            ColCount = 16;
            RowCount = 9;

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
                    ConnectedComputer newClient = new ConnectedComputer(-1, client, null);

                    StreamReader rdr = new StreamReader(client.GetStream());
                    newClient.Number = Convert.ToInt32(rdr.ReadLine());

                    if (newClient.Number <= 0)
                        throw new Exception("Number can't be less or equal to zero");

                    if (clientList.Find(c => c.IP == newClient.IP) != null)
                        continue;

                    clientList.Add(newClient);
                    Invoke(new Action(() => lbConnected.Items.Add("#" + newClient.Number + " - " + Dns.GetHostEntry(newClient.IP).HostName)));
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

            ColCount = 16 * (clientList.Count + 1);

            snake = new Snake(0, 1, Screen.PrimaryScreen.Bounds.Width / 16, Screen.PrimaryScreen.Bounds.Height / 9);
            snake.EatFruit();
            snake.EatFruit();
            snake.EatFruit();
            snake.EatFruit();
            snake.EatFruit();
            snake.EatFruit();
            snake.EatFruit();
            snake.EatFruit();

            this.KeyDown += ChangeDirection;
            this.Controls.Remove(this.pnlConnect);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Bounds = new Rectangle(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            this.TransparencyKey = Color.Fuchsia;
            this.BackColor = Color.Fuchsia;
            this.g = this.CreateGraphics();

            Frame();

            Timer t = new Timer();
            t.Interval = 75;
            t.Tick += (object o, EventArgs ev) => Frame();
            t.Start();
        }

        private void ChangeDirection(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Right: xDir = xDirection.Right; yDir = yDirection.None; break;
                case Keys.Left:  xDir = xDirection.Left;  yDir = yDirection.None; break;
                case Keys.Up:    xDir = xDirection.None;  yDir = yDirection.Up;   break;
                case Keys.Down:  xDir = xDirection.None;  yDir = yDirection.Down; break;
            }
        }

        private void Frame()
        {
            g.Clear(this.TransparencyKey);

            List<String> tempStreams = new List<String>();
            for (int n = 0; n < clientList.Count; n++)
                tempStreams.Add("");


            ///////////////
            // HEAD DRAW //
            ///////////////
            int client = (int)(snake.Head.X / 16f) - 1;
            if (client < 0)
                g.FillRectangle(Brushes.Black, new Rectangle(snake.Head.X * snake.Scale.Width, snake.Head.Y * snake.Scale.Height, snake.Scale.Width, snake.Scale.Height));
            else
                tempStreams[client] += (snake.Head.X - (client + 1) * 16) * snake.Scale.Width + "," + snake.Head.Y * snake.Scale.Height + "," + snake.Scale.Width + "," + snake.Scale.Height + "-";


            ///////////////
            // TAIL DRAW //
            ///////////////
            client = -1;
            foreach (Point p in snake.Tail)
            {
                int newClient = (int)(p.X / 16f) - 1;
                if (newClient < 0)
                    g.FillRectangle(Brushes.Black, new Rectangle(p.X * snake.Scale.Width, p.Y * snake.Scale.Height, snake.Scale.Width, snake.Scale.Height));
                else
                {
                    StreamWriter writer = clientList[newClient].ClientWriter;
                    if (newClient != client)
                        client = newClient;

                    tempStreams[client] += (p.X - (client + 1) * 16) * snake.Scale.Width + "," + p.Y * snake.Scale.Height + "," + snake.Scale.Width + "," + snake.Scale.Height + "-";
                }
            }


            ////////////////
            // FRUIT DRAW //
            ////////////////
            client = (int)(snake.Fruit.X / 16f) - 1;
            if (client < 0)
                g.FillRectangle(Brushes.Red, new Rectangle(snake.Fruit.X * snake.Scale.Width, snake.Fruit.Y * snake.Scale.Height, snake.Scale.Width, snake.Scale.Height));
            else
                tempStreams[client] += ";" + ((snake.Fruit.X - (client + 1) * 16) * snake.Scale.Width + "," + snake.Fruit.Y * snake.Scale.Height + "," + snake.Scale.Width + "," + snake.Scale.Height);


            for (int n = 0; n < tempStreams.Count; n++)
                clientList[n].ClientWriter.WriteLine(tempStreams[n]);

            snake.Update(xDir, yDir);
        }

        private int ClientCompare(ConnectedComputer c1, ConnectedComputer c2)
        {
            return c1.Number.CompareTo(c2.Number);
        }
    }

    class ConnectedComputer
    {
        public int Number { get; set; }
        public IPAddress IP { get; set; }
        public TcpClient Client { get; set; }
        public StreamWriter ClientWriter { get; set; }
        public Graphics ClientGraphics { get; set; }

        public ConnectedComputer(int n, TcpClient c, Graphics g)
        {
            this.Number = n;
            this.IP = ((IPEndPoint)c.Client.RemoteEndPoint).Address;
            this.Client = c;
            this.ClientWriter = new StreamWriter(c.GetStream());
            this.ClientWriter.AutoFlush = true;
            this.ClientGraphics = g;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace server
{
    enum xDirection { Left = -1, Right = 1, None = 0 }
    enum yDirection { Up = -1, Down = 1, None = 0 }

    class Snake
    {
        public List<Point> Tail { get; private set; }
        public Size Scale { get; set; }
        public Point Head { get; private set; }

        public Snake(int x, int y, int scaleX, int scaleY)
        {
            Tail = new List<Point>();
            Head = new Point(x, y);

            Tail.Add(new Point(Head.X, Head.Y));
            this.Scale = new Size(scaleX, scaleY);
        }

        public void EatFruit()
        {
            Point last = Tail[Tail.Count - 1];
            Tail.Add(new Point(last.X, last.Y));
        }

        public bool Update(xDirection xd, yDirection yd)
        {
            for (int n = Tail.Count - 1; n >= 1; n--)
                Tail[n] = new Point(Tail[n - 1].X, Tail[n - 1].Y);

            if (Tail.Count >= 1)
                Tail[0] = new Point(Head.X, Head.Y);
            Head = new Point(Head.X + (int) xd, Head.Y + (int) yd);

            int x = Head.X;
            Lerp(ref x, 0, Form1.ColCount - 1);

            int y = Head.Y;
            Lerp(ref y, 0, Form1.RowCount - 1);

            Head = new Point(x, y);

            foreach (Point p in Tail)
                if (p.Equals(Head))
                    return false;

            return true;
        }

        private void Lerp (ref int n, int min, int max)
        {
            if (n < min)
                n = max;
            else if (n > max)
                n = min;
        }
    }
}

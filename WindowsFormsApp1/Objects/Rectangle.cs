using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsFormsApp1.Objects
{
    //Фигура квадрат
    class RectangleShape : ObjectShape

    {
        protected override GraphicsPath GraphicsPathObject
        {
            get
            {
                GraphicsPath path = new GraphicsPath();
                if (IsCurrent)
                {
                    Rectangle r = new Rectangle();
                    if (this.p2.X < this.p1.X && this.p2.Y < this.p1.Y) 
                    {
                        r = new Rectangle(this.p2, new Size(this.p1.X - this.p2.X, this.p1.Y - this.p2.Y));
                    }
                    else if (this.p1.X > this.p2.X && this.p1.Y < this.p2.Y)
                    {
                        r = new System.Drawing.Rectangle(new Point(this.p2.X, this.p1.Y), new Size(this.p1.X - this.p2.X, this.p2.Y - this.p1.Y));
                    }
                    else if (this.p1.X < this.p2.X && this.p1.Y > this.p2.Y)
                    {
                        r = new System.Drawing.Rectangle(new Point(this.p1.X, this.p2.Y), new Size(this.p2.X - this.p1.X, this.p1.Y - this.p2.Y));
                    }
                    else
                    {
                        r = new System.Drawing.Rectangle(this.p1, new Size(this.p2.X - this.p1.X, this.p2.Y - this.p1.Y));
                    }
                    Points = new List<PointF>(){ new Point(r.Left, r.Top),  new Point(r.Right, r.Top),
                   new Point(r.Right, r.Bottom), new Point(r.Left, r.Bottom) };
                }
                if(Points.Count == 4)
                    path.AddPolygon(Points.ToArray());
                return path;
            }
        }
        protected GraphicsPath GraphicsPathPoint
        {
            get
            {
                GraphicsPath path = new GraphicsPath();
                if (this.p2.X < this.p1.X && this.p2.Y < this.p1.Y) 
                {
                    path.AddRectangle(new Rectangle(p2.X - 6, p2.Y - 6, 6, 6));
                }
                else if (this.p1.X > this.p2.X && this.p1.Y < this.p2.Y) 
                {
                    path.AddRectangle(new Rectangle(p2.X - 6, p2.Y, 6, 6));
                }
                else if (this.p1.X < this.p2.X && this.p1.Y > this.p2.Y) 
                {
                    path.AddRectangle(new Rectangle(p2.X, p2.Y - 6, 6, 6));
                }
                else
                {
                    path.AddRectangle(new Rectangle(p2.X, p2.Y, 6, 6));
                }
                return path;
            }
        }

        protected override Rectangle GetBorder()
        {
            PointF _p1 = GetMinPoint();
            PointF _p2 = GetMaxPoint();
            p1 = new Point((int)_p1.X, (int)_p1.Y);
            p2 = new Point((int)_p2.X, (int)_p2.Y);
            return new Rectangle(p1.X - BORDER_MARGIN, p1.Y - BORDER_MARGIN, p2.X - p1.X + BORDER_MARGIN * 2, p2.Y - p1.Y + BORDER_MARGIN * 2);
        }


        public override void Draw(Graphics myGp)
        {
            UpdatePoints(myGp);
            myGp.DrawPath(pen, GraphicsPathObject);
        }

        protected void UpdatePoints(Graphics myGp)
        {
            base.Draw(myGp);
            if (IsFilled)
            {
                using (SolidBrush sbrush = new SolidBrush(this.sbrush.Color))
                {
                    FillPath(myGp, sbrush);
                }
            }
            using (Pen pen = new Pen(this.pen.Color, this.pen.Width) { DashStyle = this.pen.DashStyle })
            {
                if (IsCurrent && Control.ModifierKeys == Keys.Shift)
                {
                    if (this.p2.X > this.p1.X && this.p2.Y > this.p1.Y)
                    {
                        p2 = new Point(p1.X - p1.Y + p2.Y, p2.Y);
                    }
                    else if (this.p1.X < this.p2.X && this.p1.Y > this.p2.Y)
                    {
                        int total = (p1.Y - p2.Y);
                        p2 = new Point(p1.X + total, p2.Y);

                    }
                    else if (this.p1.X > this.p2.X && this.p1.Y < this.p2.Y)
                    {
                        int total = (p2.Y - p1.Y);
                        p2 = new Point(p1.X - total, p2.Y);
                    }
                    else
                    {
                        p2 = new Point(p1.X - p1.Y + p2.Y, p2.Y);
                    }

                }
            }
        }

        public override void FillPoint(Graphics myGp)
        {
        }


        public override bool IsHit(Point point)
        {
            bool res = false;
            using (GraphicsPath path = GraphicsPathObject)
            {
                    res = path.IsVisible(point);
            }
            return res;
        }

        public override bool IsHitPoint(Point point)
        {
            bool res = false;
            using (GraphicsPath path = GraphicsPathPoint)
            {
                res = path.IsVisible(point);
            }
            if(res)
                Console.WriteLine();
            return res;
        }
        public override void FillPath(Graphics myGp, SolidBrush sbrush)
        {
            using (GraphicsPath path = GraphicsPathObject)
            {

                myGp.FillPath(sbrush, path);

            }
        }
        public override void Move(Point distance)
        {
            this.p1 = new Point(this.p1.X + distance.X, this.p1.Y + distance.Y);
            this.p2 = new Point(this.p2.X + distance.X, this.p2.Y + distance.Y);
            for (int i = 0; i < this.Points.Count; i++)
            {
                this.Points[i] = new PointF(this.Points[i].X + distance.X, this.Points[i].Y + distance.Y);
            }
        }

        public override void Resize(Point mouse, Point last)
        {
            base.Resize(mouse, last);
            p1 = FloatToInt(Points[0]);
            p2 = FloatToInt(Points[1]);
        }

        public override object Clone()
        {
            RectangleShape r = new RectangleShape();
            SetBasicParameters(r);
            return r;
        }


    }
}

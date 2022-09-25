using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1.Objects
{
    //Фигура эллипс
    internal class EllipseShape : ObjectShape
    {
        protected override GraphicsPath GraphicsPathObject
        {
            get
            {
                GraphicsPath path = new GraphicsPath();
                Rectangle r = new Rectangle(p1, new Size(p2.X - p1.X, p2.Y - p1.Y));
                if (IsCurrent)
                {
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
                    Points = new List<PointF>(){ new PointF(r.Left, r.Bottom-((r.Bottom - r.Top)/2)),  new Point(r.Right - (r.Right - r.Left)/2, r.Top),
                   new Point(r.Right, r.Bottom - (r.Bottom - r.Top)/2), new Point(r.Right - (r.Right - r.Left)/2, r.Bottom) };
                }
                path.AddClosedCurve(Points.ToArray(), 0.8f);
                path.FillMode = FillMode.Alternate;
                
                return path;
            }
        }
        protected GraphicsPath GraphicsPathPoint
        {
            get
            {
                GraphicsPath path = new GraphicsPath();
                path.AddRectangle(new System.Drawing.Rectangle(p2.X - 2, p2.Y - 2, 5, 5));
                return path;
            }
        }

        protected override Rectangle GetBorder()
        {
            RectangleF r = GraphicsPathObject.GetBounds();
            return new Rectangle((int)r.X - BORDER_MARGIN, (int)r.Y - BORDER_MARGIN, (int)r.Width + BORDER_MARGIN * 2, (int)r.Height + BORDER_MARGIN * 2);
        }

        public override void Draw(Graphics myGp)
        {
   
            if(Points.Count > 2 && Angle > 0)
            {
                base.Draw(myGp);
                p1 = FloatToInt(Points[0]);
                p2 = FloatToInt(Points[3]);
            }
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
                        p2 = new Point(p1.X - p1.Y + p2.Y, p2.Y);
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
                        p2 = new Point(p1.X - p1.Y + p2.Y, p2.Y);
                }
                myGp.DrawPath(pen, GraphicsPathObject);
            }
        }

        public override void FillPoint(Graphics myGp)
        {
            throw new NotImplementedException();
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

        public override void FillPath(Graphics myGp, SolidBrush sbrush)
        {
            using (GraphicsPath path = GraphicsPathObject)
            {

                myGp.FillPath(sbrush, path);

            }
        }
        public override bool IsHitPoint(Point point)
        {
            bool res = false;
            using (GraphicsPath path = GraphicsPathPoint)
            {
                res = path.IsVisible(point);
            }
            return res;
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

        public override object Clone()
        {
            EllipseShape s = new EllipseShape();
            SetBasicParameters(s);
            return s;
        }
    }
}

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
    //Фигура треугольник
    internal class TriangleShape : ObjectShape
    {

        protected override GraphicsPath GraphicsPathObject
        {
            get
            {
                GraphicsPath path = new GraphicsPath();
                if (IsCurrent)
                {
                   
                    if (this.p2.X < this.p1.X && this.p2.Y < this.p1.Y)
                        Points = new List<PointF>() { new Point(this.p2.X, this.p1.Y),
                              new Point(((this.p2.X - this.p1.X) / 2) + this.p1.X, this.p2.Y),
                              this.p1};
                    else if (this.p1.X > this.p2.X && this.p1.Y < this.p2.Y)
                        Points = new List<PointF>() { new Point(this.p1.X, this.p2.Y),
                              new Point(((this.p2.X - this.p1.X) / 2) + this.p1.X, this.p1.Y),
                              this.p2};
                    else if (this.p1.X < this.p2.X && this.p1.Y > this.p2.Y)
                        Points = new List<PointF>() { new Point(this.p1.X, this.p1.Y),
                              new Point(((this.p2.X - this.p1.X) / 2) + this.p1.X, this.p2.Y),
                              new Point(this.p2.X, this.p1.Y)};
                    else
                        Points = new List<PointF>() { new Point(this.p1.X, this.p2.Y),
                              new Point(((this.p2.X - this.p1.X) / 2) + this.p1.X, this.p1.Y),
                              this.p2};
                }
                if(Points.Count > 0)
                 path.AddPolygon(Points.ToArray());
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
            PointF _p1 = GetMinPoint();
            PointF _p2 = GetMaxPoint();
            p1 = new Point((int)_p1.X, (int)_p1.Y);
            p2 = new Point((int)_p2.X, (int)_p2.Y);
            return new Rectangle(p1.X - BORDER_MARGIN, p1.Y - BORDER_MARGIN, p2.X - p1.X + BORDER_MARGIN * 2, p2.Y - p1.Y + BORDER_MARGIN * 2);
        }

        public override void Draw(Graphics myGp)
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
                        p2 = new Point(p1.X - p1.Y + p2.Y, p2.Y);

                }
                myGp.DrawPath(pen, GraphicsPathObject);
            }
        }

        public override void FillPoint(Graphics myGp)
        {
            throw new NotImplementedException();
        }
        public override void FillPath(Graphics myGp, SolidBrush sbrush)
        {
            using (GraphicsPath path = GraphicsPathObject)
            {
                
                    myGp.FillPath(sbrush, path);
                
            }
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
            return false;
        }

        public override void Move(Point distance)
        {
            for(int i = 0; i < Points.Count; i++)
            {
                Point intP = FloatToInt(Points[i]);
                Points[i] = new Point(intP.X + distance.X, intP.Y + distance.Y);
            }
        }

        public override void Resize(Point mouse, Point last)
        {
            base.Resize(mouse, last);
        }

        public override object Clone()
        {
            TriangleShape ts = new TriangleShape();
            SetBasicParameters(ts);
            return ts;
        }
    }
}

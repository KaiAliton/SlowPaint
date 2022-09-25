using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsFormsApp1.Objects
{
    //Фигура многоугольник
    internal class PolygonShape : ObjectShape
    {

        protected override GraphicsPath GraphicsPathObject
        {
            get
            {
                GraphicsPath path = new GraphicsPath();
                if(this.Points.Count > 2)
                    path.AddPolygon(Points.ToArray());
                if (this.Points.Count > 1)
                    path.AddLine(this.Points[0], this.Points[1]);
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
                myGp.DrawPath(pen, GraphicsPathObject);
            }
        }
        public override void FillPath(Graphics myGp, SolidBrush sbrush)
        {
            using (GraphicsPath path = GraphicsPathObject)
            {

                myGp.FillPath(sbrush, path);

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

        public override bool IsHitPoint(Point point)
        {
            return false;
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


        public override void DrawBorder(Graphics myGp)
        {
           
            ControlPaint.DrawBorder(myGp,
                GetBorder(),
                Color.BlueViolet, ButtonBorderStyle.Dashed);
        }

        protected override Rectangle GetBorder()
        {
            PointF _p1 = GetMinPoint();
            PointF _p2 = GetMaxPoint();
            p1 = new Point((int)_p1.X, (int)_p1.Y);
            p2 = new Point((int)_p2.X, (int)_p2.Y);
            return new Rectangle(p1.X - BORDER_MARGIN, p1.Y - BORDER_MARGIN, p2.X - p1.X + BORDER_MARGIN * 2, p2.Y - p1.Y + BORDER_MARGIN * 2);
        }

        public override object Clone()
        {
            PolygonShape s = new PolygonShape();
            SetBasicParameters(s);
            return s;
        }

    }
}

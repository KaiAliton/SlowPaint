using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsFormsApp1.Objects
{
    //Фигура безье
    class BezierShape : ObjectShape
    {
        protected override GraphicsPath GraphicsPathObject
        {
            get
            {

                GraphicsPath path = new GraphicsPath();
                if(Points.Count < 3) path.AddLine(this.p1, this.p2);
                if (Points.Count > 2) path.AddCurve(Points.ToArray());
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
                using (Pen pen = new Pen(this.pen.Color, this.pen.Width + 3))
                {
                    res = path.IsOutlineVisible(point, pen);
                }
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
        public override void DrawBorder(Graphics myGp)
        {
            
          
            ControlPaint.DrawBorder(myGp,
                GetBorder(),
                Color.BlueViolet, ButtonBorderStyle.Dashed);
        }

        protected override Rectangle GetBorder()
        {
            RectangleF r = GraphicsPathObject.GetBounds();
            return new Rectangle((int)r.X - BORDER_MARGIN, (int)r.Y - BORDER_MARGIN, (int)r.Width + BORDER_MARGIN * 2, (int)r.Height + BORDER_MARGIN * 2);
        }

        public override object Clone()
        {
            BezierShape s = new BezierShape();
            SetBasicParameters(s);
            return s;
        }
    }
}

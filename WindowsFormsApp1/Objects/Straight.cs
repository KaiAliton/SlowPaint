using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsFormsApp1.Objects
{
    //Фигура линия
    class StraightShape : ObjectShape
    {
        protected override GraphicsPath GraphicsPathObject
        {
            get
            {

                GraphicsPath path = new GraphicsPath();

                path.AddLine(this.p1, this.p2);
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
            SetPoints();
            if(Angle != 180 && Angle != 360)
                base.Draw(myGp);
            SetFromPoints();
            using (Pen pen = new Pen(this.pen.Color, this.pen.Width) { DashStyle = this.pen.DashStyle })
            {    
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
        public override void FillPath(Graphics myGp, SolidBrush sbrush)
        {
           
        }
        public override void Move(Point distance)
        {
            this.p1 = new Point(this.p1.X + distance.X, this.p1.Y + distance.Y);
            this.p2 = new Point(this.p2.X + distance.X, this.p2.Y + distance.Y);
        }

        public override void DrawBorder(Graphics myGp)
        {
           
            ControlPaint.DrawBorder(myGp,
                GetBorder(),
                Color.BlueViolet, ButtonBorderStyle.Dashed);
        }

        private Point GetPoint2()
        {
            int maxX = p2.X;
            int maxY = p2.Y;
            if (p1.X > p2.X)
                maxX = p1.X;
            if (p1.Y > p2.Y)
                maxY = p1.Y;
            return new Point(maxX, maxY);
        }

        private Point GetPoint1()
        {
            int minX = p1.X;
            int minY = p1.Y;
            if (p1.X > p2.X)
                minX = p2.X;
            if (p1.Y > p2.Y)
                minY = p2.Y;
            return new Point(minX, minY);
        }

        protected override Rectangle GetBorder()
        {
            Point _p1 = GetPoint1();
            Point _p2 = GetPoint2();
            return new Rectangle(_p1.X - BORDER_MARGIN, _p1.Y - BORDER_MARGIN, _p2.X - _p1.X + BORDER_MARGIN * 2, _p2.Y - _p1.Y + BORDER_MARGIN * 2);
        }

        private void SetPoints()
        {
            Points = new List<PointF>() { p2, p1 };
            if (p1.X > p2.X)
                Points = new List<PointF>() { p1, p2 };
        }
        private void SetFromPoints()
        {
            if (p1.X > p2.X)
            {
                p1 = FloatToInt(Points[0]);
                p2 = FloatToInt(Points[1]);
            }
            else
            {
                p1 = FloatToInt(Points[1]);
                p2 = FloatToInt(Points[0]);
            }
        }

        public override void Resize(Point mouse, Point last)
        {
            SetPoints();
            base.Resize(mouse, last);
            SetFromPoints();
          
        }

        public override object Clone()
        {
            StraightShape s = new StraightShape();
            SetBasicParameters(s);
            return s;
        }
    }
}

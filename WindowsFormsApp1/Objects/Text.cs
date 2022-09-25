using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsFormsApp1.Objects
{

    //Фигура текст
    internal class TextShape : RectangleShape
    {
        [JsonProperty]
        public string Text { get; set; }
        [JsonProperty]
        public Font Font { get; set; } = new Font("Arial", 16);
        public TextShape()
        {
            p1 = new Point(-1, -1);
            p2 = new Point(-1, -1);
        }

        protected override GraphicsPath GraphicsPathObject
        {
            get
            {
                GraphicsPath path = new GraphicsPath();
                if (string.IsNullOrWhiteSpace(Text))
                    return path;
                path.AddString(this.Text, this.Font.FontFamily, (int)this.Font.Style, this.Font.Size+3, p1, StringFormat.GenericDefault);
                Points = new List<PointF>() { p1, p2 };
                return path;
            }
        }



        public override void Draw(Graphics myGp)
        {
            myGp.DrawString(this.Text, this.Font, pen.Brush, new Point(p1.X - 5, p1.Y - 5), StringFormat.GenericDefault);
            
        }

        public override bool IsHit(Point point)
        {
            Size stringSize = new Size();
            stringSize = TextRenderer.MeasureText(this.Text, this.Font);
            var gp = new GraphicsPath();
            var r = new Rectangle(p1, stringSize);
            gp.AddRectangle(r);
            Points = new List<PointF>() { p1, p2 };
            return gp.IsVisible(point);

        }

        public override void Resize(Point mouse, Point last)
        {
            
        }

        public override bool IsHitBorder(Point mouse)
        {
            return false;
        }
        public override void DrawBorder(Graphics myGp)
        {  
            Size stringSize = new Size();
            stringSize = TextRenderer.MeasureText(this.Text, this.Font);
            ControlPaint.DrawBorder(myGp, new Rectangle(p1.X-7, p1.Y-7,stringSize.Width, stringSize.Height), Color.BlueViolet, ButtonBorderStyle.Dashed);
        }

        public override void Move(Point distance)
        {
            Points = new List<PointF>() { p1, p2 };
            base.Move(distance);
        }

        public override object Clone()
        {
            TextShape ts = new TextShape();
            ts.Text = Text;
            ts.Font = Font;
            SetBasicParameters(ts);
            return ts;
        }
    }
}

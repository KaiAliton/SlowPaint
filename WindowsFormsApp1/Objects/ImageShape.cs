using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WindowsFormsApp1.Objects
{
    //Фигура - изображение
    class ImageShape: RectangleShape
    {

        private const int INIT_POINT = 20;

        [JsonProperty]
        public string Path { get; set; } = "";

        private Image Image { get; set; } = null;


        public ImageShape(string path)
        {
            this.p1 = new Point(INIT_POINT, INIT_POINT);
            Path = path;
            Image = Image.FromFile(path);
            this.p2 = new Point(Image.Width + this.p1.X, Image.Height + this.p1.Y);
            IsCurrent = true;
        }


        public override void Draw(Graphics myGp)
        {
            UpdatePoints(myGp);
            var update = GraphicsPathObject;
            myGp.DrawImage(Image, new PointF[] { Points[0], Points[1], Points[3] });
        }

        public override object Clone()
        {
            ImageShape @is = new ImageShape(Path);
            SetBasicParameters(@is);
            return @is;
        }
    }
}

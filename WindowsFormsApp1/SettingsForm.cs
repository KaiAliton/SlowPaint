using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using WindowsFormsApp1.Objects;

namespace WindowsFormsApp1
{
    //Форма настроек изображения
    public partial class SettingsForm : Form
    {
        //форма, которая вызывает форму настроек
        Form1 callingForm { get; set; }
        //сохраненный битмап, чтобы не было искажений при зуме и анзуме
        Bitmap originalBitmap { get; set; } = null;
        //фигуры
        List<ObjectShape> shapes = new List<ObjectShape>();
        //заливки
        List<SolidBrush> sBrushes = new List<SolidBrush>();
        //цвета контуров
        List<Color> pColors = new List<Color>();
        public SettingsForm(Form1 callingForm)
        {
            InitializeComponent();
            shapes = callingForm.currentShapes;
            this.callingForm = callingForm;
            PictureBox pcBox = (callingForm.Controls[0] as Control).Controls["pictureBox1"] as PictureBox;
            originalBitmap = callingForm.bm;
            pictureBox1.Image = originalBitmap;
            foreach (var s in callingForm.currentShapes)
            {
                sBrushes.Add(s.sbrush);
                pColors.Add(s.pen.Color);
            }
        }
        //закрытие формы
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            callingForm.InitFromBitmap(new Bitmap(pictureBox1.Image));
            base.OnFormClosed(e);
        }

        //нажатие на фильтр: яркости
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            string text = (sender as ToolStripMenuItem).Text.Replace("%", "");
            float value = int.Parse(text) * 0.01f;
            SetImgParameters(value);
        }
        //проверка не переполняет ли значение цвета 255
        private int OverflowColor(float value)
        {
            if (value > 255)
                return 255;
            return (int)value;
        }
        //установка фильтров для изображения
        private void SetImgParameters(float brightness = 0, float contrast = 1)
        {
            float[][] colorMatrixElements = {
                new float[] {contrast,0,0,0,0},
                new float[] {0,contrast,0,0,0},
                new float[] {0,0,contrast,0,0},
                new float[] {0,0,0,contrast,0},
                new float[] { brightness, brightness, brightness, 0,1.0f}
                };
            for(int i = 0; i < shapes.Count; i++)
            {
                shapes[i].pen.Color = ControlPaint.Light(pColors[i], brightness);


                if (sBrushes[i] == null)
                    continue;
                shapes[i].sbrush = new SolidBrush(ControlPaint.Light(sBrushes[i].Color, brightness));
                if (contrast > 1)
                {
                    float r = OverflowColor(sBrushes[i].Color.R * contrast);
                    float g = OverflowColor(sBrushes[i].Color.G * contrast);
                    float b = OverflowColor(sBrushes[i].Color.B * contrast);
                    shapes[i].sbrush = new SolidBrush(Color.FromArgb((int)r, (int)g, (int)b));
                    r = OverflowColor(pColors[i].R * contrast);
                    g = OverflowColor(pColors[i].G * contrast);
                    b = OverflowColor(pColors[i].B * contrast);
                    shapes[i].pen.Color = Color.FromArgb((int)r, (int)g, (int)b);
                }

            }
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            Image _img = originalBitmap;
            Graphics _g = default(Graphics);
            Bitmap bm_dest = new Bitmap(Convert.ToInt32(_img.Width), Convert.ToInt32(_img.Height));
            _g = Graphics.FromImage(bm_dest);
            _g.DrawImage(_img, new Rectangle(0, 0, bm_dest.Width + 1, bm_dest.Height + 1), 0, 0, bm_dest.Width + 1, bm_dest.Height + 1, GraphicsUnit.Pixel, imageAttributes);
            pictureBox1.Image = bm_dest;
        }


        //нажатие на фильтр: контрастности
        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            float value = float.Parse((sender as ToolStripMenuItem).Text.Replace(".", ","));
            SetImgParameters(0, value);
        }
        //сделать чб
        private void MakeBW()
        {
            Image _img = originalBitmap;
            Graphics _g = default(Graphics);
            Bitmap bm_dest = new Bitmap(Convert.ToInt32(_img.Width), Convert.ToInt32(_img.Height));
            _g = Graphics.FromImage(bm_dest);
            _g.DrawImage(_img, new Rectangle(0, 0, bm_dest.Width + 1, bm_dest.Height + 1), 0, 0, bm_dest.Width + 1, bm_dest.Height + 1, GraphicsUnit.Pixel);
            pictureBox1.Image = ToolStripRenderer.CreateDisabledImage(bm_dest);

        }

        //нажатие на фильтр: чб
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            MakeBW();
        }
        //конвертировать изображение в сепию
        private Bitmap ToSepiaTone(Image image)
        {
            ColorMatrix cm = new ColorMatrix(new float[][]
            {
        new float[] {0.393f, 0.349f, 0.272f, 0, 0},
        new float[] {0.769f, 0.686f, 0.534f, 0, 0},
        new float[] {0.189f, 0.168f, 0.131f, 0, 0},
        new float[] { 0, 0, 0, 1, 0},
        new float[] { 0, 0, 0, 0, 1}
            });
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(cm);

            Point[] points =
            {
        new Point(0, 0),
        new Point(image.Width, 0),
        new Point(0, image.Height),
    };
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

            Bitmap bm = new Bitmap(image.Width, image.Height);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.DrawImage(image, points, rect,
                    GraphicsUnit.Pixel, attributes);
            }
            return bm;
        }

        //нажатие на фильтр: сепия
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = ToSepiaTone(originalBitmap);
        }

        //событие рендеринга фигур
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            foreach (var cs in shapes)
                if (cs != null && cs.p2.X != -1)
                {
                    cs.Draw(e.Graphics);
                }
        }
    }
}

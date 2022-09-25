using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace WindowsFormsApp1.Objects
{
    //Абстрактный класс - фигура
    public abstract class ObjectShape : ICloneable
    {
        //Константы
        protected const int BORDER_MARGIN = 10;
        protected const int BORDER_INVISIBLE_WIDTH = 3;
        protected const int MAX_COORDINATE = 10000;


        [JsonProperty]
        //Левая верхняя точка
        public Point p1 { get; set; } = new Point(-1, -1);
        [JsonProperty]
        //Правая нижняя точка
        public Point p2 { get; set; } = new Point(-1, -1);

        [JsonProperty]
        private float penWidth;

        //Ширина контура
        public float PenWidth { get => penWidth;
            set
            {
                if (pen != null)
                    pen.Width = value;
                penWidth = value;
            }
        }
        private DashStyle penDashStyle;

        //Стиль контура
        [JsonProperty]
        public DashStyle PenDashStyle
        {
            get => penDashStyle;
            set
            {
                if (pen != null)
                    pen.DashStyle = value;
                penDashStyle = value;
            }
        }
        private Color penColor = Color.Black;

        //Цвет контура
        [JsonProperty]
        public Color PenBrush
        {
            get => penColor;
            set
            {
                if (pen != null)
                    pen.Color = value;
                penColor = value;
            }
        }
        [JsonIgnoreAttribute]
        //Контур
        public Pen pen { get; set; }
        [JsonProperty]
        //Заливка
        public SolidBrush sbrush { get; set; }

        //Точки, входящие в структуру фигуры
        public List<PointF> Points { get; set; } = new List<PointF>();

        //Текущая ли фигура
        public bool IsCurrent { get; set; }
        //Выбрана ли фигура
        public bool IsSelected { get; set; }
        //Изменяется ли размер фигуры
        public bool IsResize { get; set; }
        //Изменяется ли размер фигуры в данный момент
        public bool IsResizing { get; set; }
        //Залита ли фигура
        public bool IsFilled { get; set; }
        //Угол поворота
        public float Angle { get; set; } = 0;
        //Направление изменения размера
        protected ResizeDirection ResizeDirection { get; set; } = ResizeDirection.None;

        public abstract void FillPath(Graphics myGp, SolidBrush sbrush);
        public abstract void FillPoint(Graphics myGp);
        public abstract void Move(Point distance);
        public abstract bool IsHit(Point point);
        public abstract bool IsHitPoint(Point point);
        protected abstract GraphicsPath GraphicsPathObject { get; }

        //нарисовать границы (для передвижения и ресайза)
        public virtual void DrawBorder(Graphics myGp)
        {
            ControlPaint.DrawBorder(myGp,
                GetBorder(),
                Color.BlueViolet, ButtonBorderStyle.Dashed);
        }
        //нарисовать фигуру
        public virtual void Draw(Graphics myGp)
        {
            if (Angle != 0)
            {
                Rotate();
            }
        }
        //вращать фигуру
        protected void Rotate()
        {
            float centerX = 0;
            float centerY = 0;
            foreach (var p in Points)
            {
                centerX += p.X;
                centerY += p.Y;
            }
            PointF centerP = new PointF(centerX / Points.Count, centerY / Points.Count);
            Angle = Deg(Angle);
            for (int i = 0; i < Points.Count; i++)
            {
                float x = (float)((Points[i].X - centerP.X) * Math.Cos(Angle) - (Points[i].Y - centerP.Y) * Math.Sin(Angle) + centerP.X);
                float y = (float)((Points[i].X - centerP.X) * Math.Sin(Angle) + (Points[i].Y - centerP.Y) * Math.Cos(Angle) + centerP.Y);
                Points[i] = new PointF(x, y);
            }
            Angle = 0;
        }
        //получить значение в радианах
        protected float Deg(float deg)
        {
            return (float)Math.PI * deg / 180.0f;
        }

        //задевает ли курсор границу фигуры
        public virtual bool IsHitBorder(Point mouse)
        {
            return GetBorderSide(mouse) != ResizeDirection.None;
        }

        //получить направление ресайза фигуры
        protected virtual ResizeDirection GetBorderSide(Point mouse)
        {
            Rectangle r = GetBorder();
            if (mouse.X <= r.X + BORDER_INVISIBLE_WIDTH && mouse.X >= r.X - BORDER_INVISIBLE_WIDTH)
                return ResizeDirection.L;
            if (mouse.X <= r.X + BORDER_INVISIBLE_WIDTH + r.Width && mouse.X >= r.X - BORDER_INVISIBLE_WIDTH + r.Width)
                return ResizeDirection.R;
            if (mouse.Y <= r.Y + BORDER_INVISIBLE_WIDTH && mouse.Y >= r.Y - BORDER_INVISIBLE_WIDTH)
                return ResizeDirection.T;
            if (mouse.Y <= r.Y + BORDER_INVISIBLE_WIDTH + r.Height && mouse.Y >= r.Y - BORDER_INVISIBLE_WIDTH + r.Height)
                return ResizeDirection.B;
            return ResizeDirection.None;
        }

        //установить вид курсора в зависимости от стороны ресайза
        public void SetCursorByBorderSide(Point mouse)
        {
            ResizeDirection rd = GetBorderSide(mouse);
            switch (rd)
            {
                case ResizeDirection.T:
                case ResizeDirection.B:
                    Cursor.Current = Cursors.SizeNS;
                    break;
                case ResizeDirection.L:
                case ResizeDirection.R:
                    Cursor.Current = Cursors.SizeWE;
                    break;
                default:
                    Cursor.Current = Cursors.Arrow;
                    break;
            }
        }
        //получить границу фигуры в виде квадрата
        protected virtual Rectangle GetBorder()
        {
            PointF _p1 = GetMinPoint();
            PointF _p2 = GetMaxPoint();
            p1 = new Point((int)_p1.X, (int)_p1.Y);
            p2 = new Point((int)_p2.X, (int)_p2.Y);
            return new Rectangle(p1.X - BORDER_MARGIN, p1.Y - BORDER_MARGIN, p2.X - p1.X + BORDER_MARGIN * 2, p2.Y - p1.Y + BORDER_MARGIN * 2);
        }
        //получить самую правую нижнюю точку
        protected PointF GetMaxPoint()
        {
            float maxX = 0;
            float maxY = 0;
            foreach (var p in Points)
            {
                if (p.X > maxX)
                    maxX = p.X;
                if (p.Y > maxY)
                    maxY = p.Y;
            }
            return new PointF(maxX, maxY);
        }
        //получить самую левую верхнюю точку
        protected PointF GetMinPoint()
        {
            float minX = MAX_COORDINATE;
            float minY = MAX_COORDINATE;
            foreach (var p in Points)
            {
                if (p.X < minX)
                    minX = p.X;
                if (p.Y < minY)
                    minY = p.Y;
            }
            return new PointF(minX, minY);
        }
        //изменить размер фигуры
        public virtual void Resize(Point mouse, Point last)
        {
            if (!IsResizing)
            {
                ResizeDirection = GetBorderSide(mouse);
                if (ResizeDirection == ResizeDirection.None)
                    return;
                SetCursorByBorderSide(mouse);
                IsResizing = true;
            }
            float offset_x = mouse.X - last.X;
            float offset_y = mouse.Y - last.Y;

            float left, right, top, bottom;
            GetLRTB(out left, out right, out top, out bottom);

            float new_x = left;
            float new_y = top;
            float new_width = right - left;
            float new_height = bottom - top;

            switch (ResizeDirection)
            {
                case ResizeDirection.L:
                    new_x += offset_x;
                    new_width -= offset_x;
                    break;
                case ResizeDirection.R:
                    new_width += offset_x;
                    break;
                case ResizeDirection.B:
                    new_height += offset_y;
                    break;
                case ResizeDirection.T:
                    new_y += offset_y;
                    new_height -= offset_y;
                    break;
            }

            if ((new_width <= 0) || (new_height <= 0)) return;

            UpdatePolygon(left, right, top, bottom,
                new_x, new_y, new_width, new_height);
        }
        //получить отступ со всех сторон в зависимости от координат точек
        private void GetLRTB(
            out float left, out float right,
            out float top, out float bottom)
        {
            left = Points[0].X;
            right = left;
            top = Points[0].Y;
            bottom = top;
            foreach (PointF point in Points)
            {
                if (left > point.X) left = point.X;
                if (right < point.X) right = point.X;
                if (top > point.Y) top = point.Y;
                if (bottom < point.Y) bottom = point.Y;
            }
        }
        //обновить значение точек для полигона
        private void UpdatePolygon(
            float left, float right, float top, float bottom,
            float new_x, float new_y, float new_width, float new_height)
        {
            float x_scale = new_width / (right - left);
            float y_scale = new_height / (bottom - top);

            List<PointF> new_points = new List<PointF>();
            foreach (PointF point in Points)
            {
                float x = new_x + x_scale * (point.X - left);
                float y = new_y + y_scale * (point.Y - top);
                new_points.Add(new PointF(x, y));
            }
            Points = new_points;
        }
        //конвертировать точку из дробной в целочисленную
        protected Point FloatToInt(PointF pf)
        {
            return new Point((int)pf.X, (int)pf.Y);
        }
        //метод для клонирования фигур
        public virtual object Clone()
        {
            throw new NotImplementedException();
        }
        //установка базовых параметров фигуры после клонирования
        protected void SetBasicParameters(ObjectShape s)
        {
            s.pen = pen.Clone() as System.Drawing.Pen;
            if (sbrush != null)
                s.sbrush = sbrush.Clone() as System.Drawing.SolidBrush;
            s.IsFilled = IsFilled;
            s.p1 = new Point(p1.X - 5, p1.Y - 5);
            s.p2 = new Point(p2.X - 5, p2.Y - 5); ;
            foreach (var p in Points)
                s.Points.Add(new PointF(p.X, p.Y));
        }
    }
}

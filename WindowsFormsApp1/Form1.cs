using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using WindowsFormsApp1.Objects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Runtime.CompilerServices;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form, INotifyPropertyChanged
    {

        private const string PICTURE_FILTER = "Картинки (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg";

        PaintMode currentMode;
        // режим рисования
        PaintMode CurrentMode
        {
            get { return currentMode; }
            set
            {
                if (value != currentMode)
                {
                    Cursor = Cursors.Default;
                    currentMode = value;
                    pictureBox1_DoubleClick(null, null);
                    paintBezier = false;
                    editText = false;
                    if (pictureBox1.Controls.Count > 0)
                        pictureBox1.Controls.RemoveAt(0);
                    pen.Color = prevColor;
                    OnPropertyChanged("CurrentMode");
                    
                }
            }
        }

        public void InitFromBitmap(Bitmap _bm=null)
        {
            if (_bm == null)
                bm = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            else
                bm = _bm;
            g = Graphics.FromImage(bm);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.InterpolationMode = InterpolationMode.High;
            pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            pictureBox1.Image = bm;
        }
        //толщина контура
        LineThickness CurrentLineThickness { get; set; } = LineThickness.Light;
        public Form1()
        {
            InitializeComponent();
            this.Width = 1000;
            this.Height = 700;
            InitFromBitmap();
            g.Clear(Color.White);
            pen.MiterLimit = 100;
            contextMenu = new ContextMenuStrip();
            insertMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Копировать", null, new EventHandler(copy_click));
            contextMenu.Items.Add("Вставить", null, new EventHandler(insert_click));
            insertMenu.Items.Add("Вставить", null, new EventHandler(insert_click));
            contextMenu.Items.Add("Вырезать", null, new EventHandler(cut_click));
            panel1.Controls.Add(pictureBox1);
            panel1.AutoScroll = true;
        }
        //битмап панели для рисования
        public Bitmap bm;
        //контур
        Pen pen = new Pen(Color.Black);
        //заливка
        SolidBrush sbrush = new SolidBrush(Color.Black);
        //файл для рисования графики
        Graphics g;
        //путь на файл
        string CurrentFile;
        //шрифт
        Font Font = new Font("Arial", 24);
        //происходит ли рисование
        bool paint = false;
        //рисуется ли многоугольник
        bool paintPolygon = false;
        //рисуется ли безье
        bool paintBezier = false;
        //редактируют ли текст
        bool editText = false;
        //контекстное меню для всех операций
        ContextMenuStrip contextMenu { get; set; } = null;
        // контекстное меню для вставки
        ContextMenuStrip insertMenu { get; set; } = null;
        // фигура для копирования
        ObjectShape contextMenuShape { get; set; } = null;
        //фигура в буфере
        ObjectShape bufferShape { get; set; } = null;
        //выбранные фигуры
        List<ObjectShape> selectedShapes = new List<ObjectShape>();
        //точка опускания клавиши и движения
        Point px, py;

        bool resizing = true;
        public List<ObjectShape> currentShapes = new List<ObjectShape>();

        public event PropertyChangedEventHandler PropertyChanged;
        //событие обновление значения для переменной
        protected void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        //нажатие на кнопку вырезать в контекстном меню
        private void cut_click(object sender, EventArgs e)
        {
            currentShapes.Remove(contextMenuShape);
            if (selectedShapes.Contains(contextMenuShape))
                selectedShapes.Remove(contextMenuShape);
            bufferShape = contextMenuShape.Clone() as ObjectShape;
        }

        // нажатие на кнопку копирования в контекстном меню
        private void copy_click(object sender, EventArgs e)
        {
            bufferShape = contextMenuShape.Clone() as ObjectShape;
            pictureBox1.Invalidate();
            pictureBox1.Refresh();
        }
        // нажатие на кнопку вставки в контекстном меню
        private void insert_click(object sender, EventArgs e)
        {
            if (bufferShape != null)
            {
                bufferShape.IsCurrent = true;
                currentShapes.Add(bufferShape.Clone() as ObjectShape);
            }
            pictureBox1.Invalidate();
            pictureBox1.Refresh();
        }

        // обновление текста для фигуры с текстом
        private void text_changed(object sender, EventArgs e) 
        {
            var txt = sender as TextBox;
            Size size = TextRenderer.MeasureText(txt.Text, txt.Font);
            txt.ClientSize =
                new Size(size.Width, size.Height);
        }


        //нажатие на панель с изображением (тут мы обрабатываем начало координат, если например надо провести линию
        //между двумя точками, или же это непрерываное рисование линии, как для карандаша)
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (origBmp != null)
                return;
            py = e.Location;
            var a = currentShapes;
            if(e.Button == MouseButtons.Right)
            {
                contextMenuShape = GetHitTestShape(e.Location);
                if (contextMenuShape != null)
                {
                    insertMenu.Close();
                    contextMenu.Show(pictureBox1, e.Location);
                }
                else
                {
                    contextMenu.Close();
                    insertMenu.Show(pictureBox1, e.Location);
                }
            }
            else
            {
                if (CurrentMode != PaintMode.Selection)
                {

                    if (CurrentMode != PaintMode.Pencil)
                    {
                        if (CurrentMode == PaintMode.Polygon)
                        {

                            if (!paintPolygon)
                            {
                                PolygonShape polygon = new PolygonShape();
                                polygon.IsCurrent = true;
                                polygon.pen = pen.Clone() as Pen;
                                polygon.sbrush = sbrush.Clone() as SolidBrush;
                                polygon.Points.Add(e.Location);
                                polygon.Points.Add(e.Location);
                                currentShapes.Add(polygon);
                                paintPolygon = true;
                            }
                            else
                            {
                                PolygonShape polygon = currentShapes[currentShapes.Count - 1] as PolygonShape;
                                polygon.Points[polygon.Points.Count - 1] = e.Location;
                                polygon.Points.Add(e.Location);
                            }
                        }
                        else if (CurrentMode == PaintMode.Bezier)
                        {

                            if (!paintBezier)
                            {
                                BezierShape bezier = new BezierShape();
                                bezier.IsCurrent = true;
                                bezier.pen = pen.Clone() as Pen;
                                bezier.sbrush = sbrush.Clone() as SolidBrush;
                                bezier.Points.Add(e.Location);
                                bezier.Points.Add(e.Location);
                                bezier.p1 = e.Location;
                                currentShapes.Add(bezier);
                                paintBezier = true;
                            }
                            else if ((currentShapes.Last() as BezierShape).Points.Count < 4)
                            {
                                BezierShape bezier = (currentShapes.Last() as BezierShape);
                                if (bezier.Points.Count > 1)
                                    bezier.Points.Insert(1, e.Location);
                                else
                                    bezier.Points.Add(e.Location);
                            }
                            else
                            {
                                paintBezier = false;
                            }
                        }
                        else if (CurrentMode == PaintMode.Pipette)
                        {
                            Bitmap _bm = new Bitmap(pictureBox1.Image);
                            pictureBox1.DrawToBitmap(_bm, pictureBox1.ClientRectangle);
                            Color col = _bm.GetPixel(e.Location.X, e.Location.Y);
                            pen.Color = col;
                            sbrush.Color = col;

                        }
  
                        else if (CurrentMode == PaintMode.Fill)
                        {
                            ObjectShape sh = GetHitTestShape(e.Location);
                            if (sh != null)
                            {
                                sh.sbrush = sbrush.Clone() as SolidBrush;
                                sh.IsFilled = true;
                            }
                            else
                            {
                                FloodFill(bm, e.Location.X, e.Location.Y, pen.Color);
                            }

                        }

                        else if (CurrentMode == PaintMode.Text)

                        {
                            toolStripButton10.Enabled = false;
                            TextBox txt = new TextBox();
                            txt.TextChanged += text_changed;
                            txt.Location = e.Location;
                            txt.Font = Font;
                            txt.Height = (int)Font.Size;
                            txt.LostFocus += new EventHandler(txt_leave);
                            pictureBox1.Controls.Add(txt);
                            txt.Focus();
                        }


                        else

                        {
                            ObjectShape rs = CreateShapeByMode();
                            rs.IsCurrent = true;
                            rs.pen = pen.Clone() as Pen;
                            rs.sbrush = sbrush.Clone() as SolidBrush;
                            paint = true;
                            rs.p1 = py;
                            rs.p2 = new Point(-1, -1);
                            currentShapes.Add(rs);
                        }
                    }
                }
                else if (currentShapes.Count > 0)
                {
                    if (selectedShapes.Count == 1)
                    {
                        if (selectedShapes.Count == 1 && selectedShapes[0].IsHitBorder(e.Location))
                        {
                            selectedShapes[0].IsResize = true;
                            return;
                        }
                    }
                    if (Control.ModifierKeys == Keys.Shift)
                    {
                        ObjectShape shape = GetHitTestShape(e.Location);
                        if (shape != null && !selectedShapes.Contains(shape))
                            selectedShapes.Add(shape);
                    }
                    else
                    {
                        ObjectShape shape = GetHitTestShape(e.Location);
                        if (shape != null && !selectedShapes.Contains(shape))
                            selectedShapes = new List<ObjectShape>() { shape };
                        else if (shape == null)
                            selectedShapes = new List<ObjectShape>();
                    }
                    if (selectedShapes.Count != 0)
                        Cursor.Current = Cursors.Hand;
                }
            }
            
            pictureBox1.Invalidate();
            pictureBox1.Refresh();

        }


        //получить фигуру, по координату выбора мыши (когда нажимаем на выбор, а затем левую кнопку мыши
        //необходимо понять попадает ли курсор в какую либо из фигур)
        private ObjectShape GetHitTestShape(Point p)
        {
            foreach (var shape in currentShapes)
                if (shape.IsHit(p))
                    return shape;
            return null;
        }
        //конец редактирования фигуры с текстом
        void txt_leave(object sender, EventArgs e)
        {
            toolStripButton10.Enabled = true;
            var tb = ((TextBox)sender);
            if (string.IsNullOrEmpty(tb.Text))
                return;
            TextShape textShape = new TextShape();
            textShape.Text = tb.Text;
            textShape.IsCurrent = true;
            textShape.Points.Add(tb.Location);
            textShape.Font = tb.Font.Clone() as Font;
            textShape.p1 = tb.Location;
            Size stringSize = new Size();
            stringSize = TextRenderer.MeasureText(textShape.Text, textShape.Font);
            textShape.p2 = new Point(textShape.p1.X + stringSize.Width, textShape.p1.Y + stringSize.Height);
            textShape.sbrush = sbrush;
            textShape.pen = pen.Clone() as Pen;
            currentShapes.Add(textShape);
            tb.LostFocus -= new EventHandler(txt_leave);
            pictureBox1.Controls.Remove(tb);
            tb.Dispose();
            pictureBox1.Refresh();
            pictureBox1.Invalidate();
        }
        //курсор с банкой
        Cursor fillCur = new Cursor(Properties.Resources.bucket.Handle);

        //событие движения мыши (необходимо для ресайза, движения и прочих действий, которые согласуются с позицией курсора)
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                px = e.Location;
                if (CurrentMode == PaintMode.Selection)
                {
                   
                    if (selectedShapes.Count == 1)
                    {
                        if (selectedShapes[0].IsResize)
                            selectedShapes[0].Resize(e.Location, py);
                        else
                        {
                            Point delta_p = new Point(e.Location.X - py.X, e.Location.Y - py.Y);
                            selectedShapes[0].Move(delta_p);
                        }
               
                    }
                }
                else
                {

                    if (currentShapes.Count > 0 && CurrentMode != PaintMode.Pencil && CurrentMode != PaintMode.Eraser && CurrentMode != PaintMode.Pipette && CurrentMode != PaintMode.Fill && CurrentMode != PaintMode.Brush)
                    {
                        currentShapes.Last().p2 = px;

                    }
                    else if (CurrentMode == PaintMode.Pencil || CurrentMode == PaintMode.Eraser)
                    {
                            g.DrawLine(pen, px, py);
                    }
                    else if (CurrentMode == PaintMode.Brush)
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            g.DrawLine(pen, px, py);
                    }
                    if (paintPolygon)
                    {
                        PolygonShape polygon = currentShapes[currentShapes.Count - 1] as PolygonShape;
                        polygon.Points[polygon.Points.Count - 1] = e.Location;
                    }
                    if (paintBezier)
                    {
                        BezierShape bezier = currentShapes[currentShapes.Count - 1] as BezierShape;
                        bezier.Points[bezier.Points.Count - 2] = e.Location;
                    }

                }
                py = px;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
            }
            else
            {
                if (CurrentMode == PaintMode.Selection)
                {
                    if (selectedShapes.Count == 1 && 
                        selectedShapes[0].IsHitBorder(e.Location) &&
                        !(selectedShapes[0] is GroupedShape))
                        selectedShapes[0].SetCursorByBorderSide(e.Location);
                    else if (currentShapes.Count > 0 && GetHitTestShape(e.Location) != null)
                        Cursor.Current = Cursors.Hand;
                    else
                        Cursor.Current = Cursors.Default;
                }
            }

            pictureBox1.Invalidate();
            pictureBox1.Refresh();
        }





        //Выбор фигуры
        private void toolStripDropDownButton1_DropDownOpened(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripDropDownButton btn = sender as ToolStripDropDownButton;
            btn.Image = e.ClickedItem.Image;
            SetModeByShapeName(e.ClickedItem.AccessibilityObject.Name);
            ClearSelections(null);
        }

        // выбрать режим рисовки по выбранной фигуре
        private void SetModeByShapeName(string name)
        {
            switch (name)
            {
                case "Круг":
                    CurrentMode = PaintMode.Ellipse;
                    break;
                case "Треугольник":
                    CurrentMode = PaintMode.Triangle;
                    break;
                case "Многоугольник":
                    CurrentMode = PaintMode.Polygon;
                    break;
                case "Прямая линия":
                    CurrentMode = PaintMode.Straight;
                    break;
                case "Безье":
                    CurrentMode = PaintMode.Bezier;
                    break;

                default:
                    CurrentMode = PaintMode.Rectangle;
                    break;
            }
        }
        //создать фигуру в зависимости от выбранной в панели инструментов
        private ObjectShape CreateShapeByMode()
        {
            switch (CurrentMode)
            {
                case PaintMode.Ellipse:
                    return new EllipseShape();
                case PaintMode.Triangle:
                    return new TriangleShape();
                case PaintMode.Polygon:
                    return new PolygonShape();
                case PaintMode.Straight:
                    return new StraightShape();
                case PaintMode.Bezier:
                    return new BezierShape();
                default:
                    return new RectangleShape();
            }
        }
        //очистить прошлый выбор в меню инструментов
        private void ClearSelections(string excludingName)
        {
            var items = toolStrip1.Items;
            foreach (ToolStripItem it in items)
            {
                if (it as ToolStripButton != null && it.Name != excludingName)
                    (it as ToolStripButton).Checked = false;
            }
        }
        //очистить меню выбора
        private void ClearDropDown(ToolStripDropDownButton btn, string excludingName)
        {
            var items = btn.DropDownItems;
            foreach (ToolStripItem it in items)
            {
                if (it as ToolStripMenuItem != null && it.Name != excludingName)
                    (it as ToolStripMenuItem).Checked = false;
            }
        }
        //выбор толщины контура
        private void toolStripDropDownButton2_DropDownOpened(object sender, ToolStripItemClickedEventArgs e)
        {
            ToolStripDropDownButton btn = sender as ToolStripDropDownButton;
            ClearDropDown(btn, e.ClickedItem.Name);
            (e.ClickedItem as ToolStripMenuItem).Checked = !(e.ClickedItem as ToolStripMenuItem).Checked;
            pen.Width = (int)GetLineThickness(btn);
        }
        //получить толщину контора в зависимости от выбранной в dropdown
        private LineThickness GetLineThickness(ToolStripDropDownButton btn)
        {
            string selected = "Light";
            var items = btn.DropDownItems;
            foreach (ToolStripItem it in items)
            {
                if (it as ToolStripMenuItem != null && (it as ToolStripMenuItem).Checked)
                    selected = (it as ToolStripMenuItem).Text;
            }
            switch (selected)
            {
                case "Lighter":
                    return LineThickness.Lighter;
                case "Bold":
                    return LineThickness.Bold;
                case "Bolder":
                    return LineThickness.Bolder;
                default:
                    return LineThickness.Light;
            }
        }
        //цветовая палитра
        ColorDialog cd = new ColorDialog();
        //событие вызова цветовой палитры
        private void toolStripButton1_CheckedChanged(object sender, EventArgs e)
        {
            var btn = (sender as ToolStripButton);
            if (btn.Checked)
            {
                DialogResult res = cd.ShowDialog();
                if (res == DialogResult.OK)
                {
                    pen.Color = cd.Color;
                    sbrush.Color = cd.Color;
                }
                cd.Dispose();
                btn.Checked = false;
            }
        }
        //события рисования на холсте
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (selectedShapes.Count > 0)
                foreach (var sh in selectedShapes)
                    sh.DrawBorder(e.Graphics);


            foreach (var cs in currentShapes)
                if (cs != null && cs.p2.X != -1)
                {
                    cs.Draw(e.Graphics);
                }
        }
        //событие смены значения выбора карандаша
        private void toolStripButton2_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as ToolStripButton).Checked)
                CurrentMode = PaintMode.Pencil;
        }

        //событие двойного нажатия на холст (нужно, чтобы завершить рисование многоугольника)
        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            if (paintPolygon)
            {
                paintPolygon = false;
                PolygonShape polygon = currentShapes[currentShapes.Count - 1] as PolygonShape;
                if(polygon != null)
                    polygon.Points.RemoveAt(polygon.Points.Count - 1);
            }
            if(editText)
            {
                editText = false;
            }
        }

        //событие нажатия на кнопку выбора фигур
        private void selectionBtn_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as ToolStripButton).Checked)
                CurrentMode = PaintMode.Selection;
            else
                selectedShapes = new List<ObjectShape>();

        }
        //нажатие на пипетку
        private void toolStripButton4_Click(object sender, EventArgs e) //pipette
        {
            if ((sender as ToolStripButton).Checked)
            {
                CurrentMode = PaintMode.Pipette;
                ClearSelections((sender as ToolStripButton).Name);
            }
        }
        //событие нажатия на карандаш
        private void toolStripButton2_Click(object sender, EventArgs e) //pencil
        {
            if ((sender as ToolStripButton).Checked)
            {
                CurrentMode = PaintMode.Pencil;
                ClearSelections((sender as ToolStripButton).Name);
            }
                
        }
        //событие нажатия на кисть
        private void toolStripButton3_Click(object sender, EventArgs e) //brush
        {
            if ((sender as ToolStripButton).Checked)
            {
                CurrentMode = PaintMode.Brush;
                ClearSelections((sender as ToolStripButton).Name);
            }
                

        }
        //событие нажатия на заливку
        private void toolStripButton5_Click(object sender, EventArgs e) //fill
        {
            if ((sender as ToolStripButton).Checked)
            {
                CurrentMode = PaintMode.Fill;
                Cursor = fillCur;
                ClearSelections((sender as ToolStripButton).Name);
            }
                
        }
        //событие нажатия на создание фигуры в виде текста
        private void toolStripButton7_Click(object sender, EventArgs e) //text
        {
                CurrentMode = PaintMode.Text;
                ClearSelections((sender as ToolStripButton).Name);
            
        }
        //событие нажатия поворота на 90
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            if (selectedShapes.Count == 1)
            {
                selectedShapes[0].Angle = 90.0f;
                pictureBox1.Refresh();
                pictureBox1.Invalidate();
            }
        }
        //событие нажатия поворота на 180
        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            if (selectedShapes.Count == 1)
            {
                selectedShapes[0].Angle = 180.0f;
                pictureBox1.Refresh();
                pictureBox1.Invalidate();
            }
        }

        //событие нажатия на выбор шрифта
        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            var btn = (sender as ToolStripButton);
                FontDialog fontDialog = new FontDialog();
                DialogResult res = fontDialog.ShowDialog();
                if (res == DialogResult.OK)           
                    Font = fontDialog.Font;
                fontDialog.Dispose();
        }
        //проверка чтобы в текстбокс для ввода угла поступали только численные значения
        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }
        //событие нажатия на поворот на вольный угол
        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            if (selectedShapes.Count == 1)
                if (string.IsNullOrEmpty(toolStripTextBox1.Text))
                    return;
                else
                    selectedShapes[0].Angle = int.Parse(toolStripTextBox1.Text);
            pictureBox1.Refresh();
            pictureBox1.Invalidate();
        }

   
        //создания фигуры с изображением
        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Filter = PICTURE_FILTER;
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                ImageShape imgShape = new ImageShape(ofd.FileName);
                imgShape.pen = pen.Clone() as Pen;
                currentShapes.Add(imgShape);
            }
    
        }
        //объединение фигур
        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            if (selectedShapes.Count > 1)
            {
                foreach (var sh in selectedShapes)
                {
                    currentShapes.Remove(sh);
                }
                GroupedShape gs = new GroupedShape();
                gs.Shapes = selectedShapes;
                gs.IsCurrent = true;
                gs.pen = pen.Clone() as Pen;
                gs.IsSelected = true;
                selectedShapes = new List<ObjectShape>() { gs };
                currentShapes.Add(gs);
                pictureBox1.Refresh();
                pictureBox1.Invalidate();
            }
        }
        //разъединение фигур
        private void toolStripButton14_Click(object sender, EventArgs e)
        {
            if(selectedShapes.Count > 0 && selectedShapes[0] is GroupedShape)
            {
                List<ObjectShape> shapes = (selectedShapes[0] as GroupedShape).Shapes;
                currentShapes.Remove(selectedShapes[0]);
                currentShapes.AddRange(shapes);
                selectedShapes.Clear();
                selectedShapes = shapes;
                pictureBox1.Refresh();
                pictureBox1.Invalidate();
            }
      
            
        }
        //события отпускания кнопки мыши
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (selectedShapes.Count != 0)
            {
                foreach(var sh in selectedShapes)
                {
                    sh.IsResize = false;
                    sh.IsResizing = false;
                }
            }
                
            foreach (var cs in currentShapes)
                if (cs != null)
                    cs.IsCurrent = false;
        }

        //открытие файла
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            px = new Point(-1, -1);
            py = new Point(-1, -1);
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = $"JSON файл (*.json)|*.json|{PICTURE_FILTER}";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                if(ofd.FileName.Contains(".json"))
                {
                    currentShapes = Serializer.Deserialize(ofd.FileName);
       
                    string name = Path.GetFileName(ofd.FileName);
                    string dir = Path.GetDirectoryName(ofd.FileName);
                    var splited = name.Split('.');
                    string imgPath = dir + "\\" + splited[0] + ".png";
                    if (File.Exists(imgPath))
                    {
                        bm = new Bitmap(Image.FromFile(imgPath));
                        InitFromBitmap(bm);
                    }
                    else
                        bm = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                   
                }
                else
                {
                    currentShapes = new List<ObjectShape>();
                    bm = new Bitmap(Image.FromFile(ofd.FileName));
                    InitFromBitmap(bm);
                }
                selectedShapes = new List<ObjectShape>();
                CurrentFile = ofd.FileName;
                pictureBox1.Refresh();
                pictureBox1.Invalidate();
              
            }
    
        }
        //сохранение файла
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            sf.Filter = $"JSON файл (*.json)|*.json|{PICTURE_FILTER}";
            if(sf.ShowDialog() == DialogResult.OK)
                Save(sf.FileName);
       
        }
        //переменная для сохранения значения цвета контура, после нажатия на ластик
        Color prevColor = Color.Black;
        //событие нажатия на ластик
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            prevColor = pen.Color;
            CurrentMode = PaintMode.Eraser;
            ClearSelections("Ластик");
            pen.Color = Color.White;
            Cursor = Cursors.Cross;
        }


        //событие нажатия на сохранить
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(File.Exists(CurrentFile))
            {
                Save(CurrentFile);
            }
        }

        //сохранить файл по пути path
        private void Save(string path)
        {
            selectedShapes = new List<ObjectShape>();
            if (path.Contains(".json"))
            {
                string name = Path.GetFileName(path);
                string dir = Path.GetDirectoryName(path);
                var splited = name.Split('.');
                string imgPath = dir + "\\" + splited[0] + ".png";
                bm.Save(imgPath, ImageFormat.Png);
                Serializer.Serialize(Path.GetDirectoryName(path) + "\\" + splited[0] + ".json", currentShapes);
                selectedShapes = new List<ObjectShape>();
            }
            else
            {
                Bitmap _bm = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                pictureBox1.DrawToBitmap(_bm, pictureBox1.ClientRectangle);
                _bm.Save(path, ImageFormat.Png);
            }
        }


        //показать форму настрое
        private void toolStripButton15_Click(object sender, EventArgs e)
        {
            SettingsForm f = new SettingsForm(this);
            f.ShowDialog();
        }
        //переменная для хранения оригинального битмапа
        Bitmap origBmp { get; set; }
        //скопированные фигуры
        List<ObjectShape> copiedShapes = new List<ObjectShape>();
        //зум +
        private void toolStripButton16_Click_1(object sender, EventArgs e)
        {
            if (origBmp == null)
            {
                foreach(ToolStripItem it in toolStrip1.Items)
                    it.Enabled = false;
            
                toolStripButton17.Enabled = true;
                CurrentMode = PaintMode.None;
                origBmp = new Bitmap(pictureBox1.Image);
                Bitmap originalBitmap = new Bitmap(pictureBox1.Image);
                pictureBox1.DrawToBitmap(originalBitmap, pictureBox1.ClientRectangle);
                Size newSize = new Size((int)(originalBitmap.Width * 1.5f), (int)(originalBitmap.Height * 1.5f));
                Bitmap bmp = new Bitmap(originalBitmap, newSize);
                InitFromBitmap(bmp);
                for(int i = 0; i < currentShapes.Count; i++)
                    copiedShapes.Add(currentShapes[i].Clone() as ObjectShape);
                currentShapes.Clear();
            }
        }
        //зум -
        private void toolStripButton17_Click(object sender, EventArgs e)
        {
            if (origBmp != null)
            {
                InitFromBitmap(origBmp);
                currentShapes = copiedShapes;
                origBmp = null;
                foreach (ToolStripItem it in toolStrip1.Items)
                    it.Enabled = true;
                copiedShapes = new List<ObjectShape>();
            }
        }
        //про программу
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Мой пеинт");
        }
        //выход из приложения
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        //новый файл
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.py = new Point(0, 0);
            this.px = new Point(0, 0);
            currentShapes = new List<ObjectShape>();
            selectedShapes = new List<ObjectShape>();
            prevColor = Color.Black;
            InitFromBitmap();
            g.Clear(Color.White);
            CurrentFile = "";
        }
        //заливка по координатам мыши
        void FloodFill(Bitmap bitmap, int x, int y, Color color)
        {
            BitmapData data = bitmap.LockBits
                (
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb
                );
            int[] bits = new int[data.Stride / 4 * data.Height];
            Marshal.Copy(data.Scan0, bits, 0, bits.Length);
            LinkedList<Point> check = new LinkedList<Point>();
            int floodTo = color.ToArgb();
            int floodFrom = bits[x + y * data.Stride / 4];
            bits[x + y * data.Stride / 4] = floodTo;

            if (floodFrom != floodTo)
            {
                check.AddLast(new Point(x, y));
                while (check.Count > 0)
                {
                    Point cur = check.First.Value;
                    check.RemoveFirst();

                    foreach (Point off in new Point[] {
                new Point(0, -1), new Point(0, 1),
                new Point(-1, 0), new Point(1, 0)})
                    {
                        Point next = new Point(cur.X + off.X, cur.Y + off.Y);
                        if (next.X >= 0 && next.Y >= 0 &&
                            next.X < data.Width &&
                            next.Y < data.Height)
                        {
                            if (bits[next.X + next.Y * data.Stride / 4] == floodFrom)
                            {
                                check.AddLast(next);
                                bits[next.X + next.Y * data.Stride / 4] = floodTo;
                            }
                        }
                    }
                }
            }
            Marshal.Copy(bits, 0, data.Scan0, bits.Length);
            bitmap.UnlockBits(data);
        }



    }
}

namespace WindowsFormsApp1
{
    //режим рисования
    internal enum PaintMode
    {
        Pencil = 0,
        Rectangle = 1,
        Ellipse = 2,
        Triangle = 3,
        Polygon = 4,
        Straight = 5,
        Bezier = 6,
        Selection = 7,
        Pipette = 8,
        Fill = 9,
        Brush = 10, 
        Text = 11,
        Eraser = 12,
        None = -1
    }
}

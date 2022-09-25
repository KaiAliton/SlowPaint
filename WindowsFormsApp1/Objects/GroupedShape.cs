using System;
using System.Collections.Generic;
using System.Drawing;

namespace WindowsFormsApp1.Objects
{
    //Фигура, состоящая из других фигур (при объединении)
    class GroupedShape:RectangleShape
    {
        public List<ObjectShape> Shapes { get; set; } = new List<ObjectShape>();

        public override void Resize(Point mouse, Point last) { }

        protected override Rectangle GetBorder()
        {
            UpdatePoints();
            return base.GetBorder();
        }

        public override bool IsHit(Point point)
        {
            foreach (var sh in Shapes)
                if (sh.IsHit(point))
                    return true;
            return false;
        }

        public override void Draw(Graphics myGp)
        {
            UpdatePoints();
            foreach (var sh in Shapes)
            {
                sh.Angle = Angle;
                sh.Draw(myGp);
            }
            Angle = 0;
        }

        public override void Move(Point distance)
        {
            UpdatePoints();
            foreach (var sh in Shapes)
                sh.Move(distance);
        }

        public void UpdatePoints()
        {
            Points.Clear();
            foreach (var sh in Shapes)
                Points.AddRange(sh.Points);
        }


        public override object Clone()
        {
            GroupedShape gs = new GroupedShape();
            gs.Shapes = new List<ObjectShape>();
            foreach (var s in Shapes)
                gs.Shapes.Add(s.Clone() as ObjectShape);
            SetBasicParameters(gs);
            gs.Points.Clear();
            foreach (var sh in Shapes)
                gs.Points.AddRange(sh.Points);
            return gs;
        }

    }
}

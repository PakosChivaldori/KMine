using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kmine
{
    public partial class GridField : PictureBox
    {
        /// <summary>
        /// Размер поля
        /// </summary>
        public Size FieldSize { get { return ScriptState.FieldSize; } }

        /// <summary>
        /// Общее количество М.
        /// </summary>
        public int TotalMines { get { return ScriptState.TotalMines; } }

        /// <summary>
        /// Получить ячейку по координатам
        /// </summary>
        public GridCell this[int x, int y]
        {
            get { return ScriptState.GetGridCell(x, y); }
        }

        public GridField()
        {
            InitializeComponent();
            this.Image = new Bitmap(Width, Height);
        }

        /// <summary>
        /// Рисовать поле
        /// </summary>
        public void DrawField()
        {
            if (FieldSize.Width == 0 || FieldSize.Height == 0) return;
            var deltaX = (int)(Size.Width / FieldSize.Width);
            var deltaY = (int)(Size.Height / FieldSize.Height);

            this.Image = new Bitmap(Width, Height);
            var g = Graphics.FromImage(this.Image);
            var p = new Pen(Color.Black, 1);

            g.Clear(Color.White);

            g.DrawRectangle(p, 0, 0, Size.Width - 1, Size.Height - 1);
            for (int x = 1; x < FieldSize.Width; ++x)
            {
                g.DrawLine(p, x * deltaX, 0, x * deltaX, Height);
            }
            for (int y = 1; y < FieldSize.Height; ++y)
            {
                g.DrawLine(p, 0, y * deltaY, Width, y * deltaY);
            }
            for (int y = 0; y < FieldSize.Height; ++y)
            {
                int cy = y * deltaY + (deltaY >> 1);
                for (int x = 0; x < FieldSize.Width; ++x)
                {
                    Point po = new Point(x * deltaX + (deltaX >> 1), cy);
                    var c = this[x, y];
                    Image i = null;
                    if (c.Flag)
                        i = imgs.Images[0];
                    else
                        if (c.Number > 0)
                            i = imgs.Images[c.Number];

                    if (i != null)
                    {
                        po.Offset(-(i.Width >> 1), -(i.Height >> 1));
                        g.DrawImage(i, po);
                    }
                }
            }
            Invalidate();
        }
    }
}
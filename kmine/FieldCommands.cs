using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kmine
{
    [Keyword("detectfield")]
    public class DetectField : ICommand
    {
        public string Name { get; set; }
        public Config Cfg { get; set; }
        public bool Parse(string data) { return true; }

        public DetectField(string name, Config cfg) { Name = name; Cfg = cfg; }
        public void Action(ScriptState state)
        {
            Shaft[] shafts = new[] { 
                                 new Shaft("Леонсио", "leonsio", 13, new Point(-96,169), new Size(450,350)),
                                 new Shaft("Ухта", "uhta", 17, new Point(-88,48), new Size(450,350))
                             };
            foreach (var s in shafts)
            {
                var b = global::kmine.Properties.Resources.ResourceManager.GetObject(s.image) as Bitmap;
                if (b != null)
                {
                    var x = new GImage(b);
                    var u = state.Image.Find(x, new Point());
                    if (!u.IsEmpty)
                    {
                        ScriptState.TotalMines = s.mines;
                        ScriptState.Shaft = s.name;
                        ScriptState.FieldSize = new Size(s.size.Width / 50, s.size.Height / 50);
                        state.ActionDone = true;
                        ///!!! Debug
                        //                        Gray c = new Gray(255);
                        //                        state.Image.Draw(u, c, 1);
                        u = new Rectangle(new Point(u.Left + s.delta.X, u.Top + s.delta.Y), s.size);
                        ScriptState.FieldRectangle = u;
                        //                        state.Image.Draw(u, c, 1);
                        //                        state.Image.Save("2.bmp");
                        break;
                    }
                }
                else Program.log.Error("EdgeDetect: No resource found {0}", s.image);
            }


            // !!! Задать ссылку на угол
            /*var img = new kmine.GImage(global::kmine.Properties.Resources.leonsio);
            if (Check(state, img)) { }
            var tl = state.Image.Find(Top, new Point());
            if (!tl.IsEmpty)
            {

                var TopLeft = new kmine.GImage(global::kmine.Properties.Resources.leonsio);
                tl = state.Image.Find(TopLeft, new Point());
                if (!tl.IsEmpty)
                {
                    var BottomRight = new GImage(global::kmine.Properties.Resources.greenlight);
                    var br = state.Image.Find(BottomRight, new Point());
                    if (!br.IsEmpty)
                    {

                    }
                }
            }*/
        }

        /// <summary>
        /// Проверить шахту на картинке
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private bool Check(ScriptState state, GImage img)
        {
            var tl = state.Image.Find(img, new Point());
            return false;
        }

        struct Shaft
        {
            public string name;
            public string image;
            public int mines;
            public Point delta;
            public Size size;
            public Shaft(string name, string image, int mines, Point delta, Size size) { this.name = name; this.image = image; this.mines = mines; this.delta = delta; this.size = size; }
        };
    }

    /// <summary>
    /// Проверяет определено ли поле и пока нет - дальше не пущает
    /// </summary>
    [Keyword("fielddetected")]
    public class FieldDetected : ICommand
    {
        public string Name { get; set; }
        public Config Cfg { get; set; }
        public bool Parse(string data) { return true; }

        public FieldDetected(string name, Config cfg) { Name = name; Cfg = cfg; }
        public void Action(ScriptState state)
        {
            state.Running = !ScriptState.FieldRectangle.IsEmpty;
        }
    }

    [Keyword("fieldcells")]
    public class FieldCells : ICommand
    {
        public string Name { get; set; }
        public Config Cfg { get; set; }
        public bool Parse(string data) { return true; }
        GImage[] nums;

        public FieldCells(string name, Config cfg) { 
            Name = name; Cfg = cfg;
            nums = new GImage[10];
            nums[1] = ToGray(global::kmine.Properties.Resources._1);
            nums[2] = ToGray(global::kmine.Properties.Resources._2);
            nums[3] = ToGray(global::kmine.Properties.Resources._3);
        }

        GImage ToGray(GImage src)
        {
            return new GImage(src.InRange(new Emgu.CV.Structure.Gray(0), new Emgu.CV.Structure.Gray(100)).Bitmap);
        }

        GImage ToGray(Bitmap bmp)
        {
            Bitmap x = new Bitmap(bmp.Size.Width, bmp.Size.Height);
            using (var g = Graphics.FromImage(x))
            {
                g.Clear(Color.Black);
                g.DrawImage(bmp, Point.Empty);
            }
            return ToGray(new GImage(x));
        }

        public void Action(ScriptState state)
        {
            state.Image.ROI = ScriptState.FieldRectangle;
            var i = ToGray(state.Image);
            var ok = false;

            for (int y = 0; y < ScriptState.FieldSize.Height; ++y)
                for (int x = 0; x < ScriptState.FieldSize.Width; ++x)
                {
                    i.ROI = new Rectangle(new Point(x * 50, y * 50), new Size(50, 50));
                    var h = new Emgu.CV.DenseHistogram(256, new RangeF(0f, 255f));
                    h.Calculate(new[] { i }, false, null);
                    var valHist = new float[256];
                    h.MatND.ManagedArray.CopyTo(valHist, 0);
                    if (valHist[0] < 1000)
                    {
                        for (int n = 0; n < nums.Length; ++n)
                        {
                            if (nums[n] != null)
                            {
                                var r = i.Find(nums[n], Point.Empty);
                                if (!r.IsEmpty)
                                {
                                    state[x, y].Number = n >= 0 && n <= 9 ? n : n; // !!!! случай флага и незанятой клетки
                                    ok = true;
                                }
                            }
                        }
                    }
                    else state[x, y].Number = -1;
                }
            if (ok) state.ActionDone = true;
        }
    }
}
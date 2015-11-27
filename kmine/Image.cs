using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Cvb;

namespace kmine
{
    // Чтобы везде было одинаково
    public class GImage : Image<Gray, Byte>
    {
        public GImage(Bitmap img) : base(img) { }
        public GImage(string img) : base(img) { }

        /// <summary>
        /// Поиск совпадений
        /// </summary>
        /// <param name="f">Что ищем</param>
        /// <param name="offset">Смещение !!! пока не работает</param>
        /// <returns>Совпадающий блок</returns>
        public Rectangle Find(GImage f, Point offset)
        {
            if (Width < f.Width || Height < f.Height) return new Rectangle();
            var r = MatchTemplate(f, TM_TYPE.CV_TM_CCOEFF_NORMED);

            double[] minValues, maxValues;
            Point[] minLocations, maxLocations;
            r.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);
            Rectangle match = new Rectangle();
            // You can try different values of the threshold. I guess somewhere between 0.75 and 0.95 would be good.
            if (maxValues[0] > 0.9)
            {
                match = new Rectangle(maxLocations[0], f.Size);
            }

            return match;
        }

        /// <summary>
        /// Захват экрана
        /// </summary>
        /// <param name="config">Конфиг для уточнения размера и координат окна захвата</param>
        static public Bitmap Capture(Config config)
        {
            var bmp = new Bitmap(config.maxWidth, config.maxHeight);
            var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.CopyFromScreen(new Point(config.xOffset, config.yOffset), new Point(0, 0), new Size(bmp.Width, bmp.Height));
            bmp.Save("1.bmp");
            return bmp;
        }

        [DllImport("user32.dll")]
        static extern IntPtr SetForegroundWindow(IntPtr hWnd);
        /// <summary>
        /// Найти нужное окно
        /// </summary>
        public static bool FindProper(Config config)
        {
            if (string.IsNullOrEmpty(config.WindowName))
                return false;
            foreach (var proc in System.Diagnostics.Process.GetProcesses()) // перебираем все процесы
            {
                if (proc.MainWindowTitle.ToString().StartsWith(config.WindowName)) // находим окно по точному заголовку окна
                {
                    Program.log.Info("Окно найдено");
                    SetForegroundWindow(proc.MainWindowHandle);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Найти окно с флешем
        /// </summary>
        /// <param name="config">Ссылка на конфиг</param>
        /// <param name="setX">устанавливать x</param>
        /// <param name="setY">устанавливать y</param>
        /// <param name="bmp">скриншот</param>
        public static void FindFlash(Config config, bool setX, bool setY, Bitmap bmp)
        {
            var r = new GImage(bmp).Find(new GImage(global::kmine.Properties.Resources.klohead), Point.Empty);
            bmp.Save("x.bmp");
            if (!r.IsEmpty)
            {
                r.Offset(-270, -20);
                if (setX) config.xOffset += r.Left;
                if (setY) config.yOffset += r.Top;
            }
        }
    }
}
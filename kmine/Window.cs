using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;

namespace kmine
{
    public class Window
    {
        public IntPtr Handle { get; set; }

        private Config config;

        public Window(Config config)
        {
            this.config = config;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
        [DllImport("USER32.DLL")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        static extern IntPtr SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr dc);
        [DllImport("gdi32.dll")]
        public static extern UInt64 BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, System.Int32 dwRop);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        /// <summary>
        /// Найти нужное окно
        /// </summary>
        public bool FindProper()
        {
            Handle = IntPtr.Zero;
            if (string.IsNullOrEmpty(config.WindowName))
                return false;
            foreach (var proc in System.Diagnostics.Process.GetProcesses()) // перебираем все процесы
            {
                if (proc.MainWindowTitle.ToString().StartsWith(config.WindowName)) // находим окно по точному заголовку окна
                {
                    Program.log.Info("Окно найдено");
                    Handle = proc.MainWindowHandle;
                    //SetForegroundWindow(Handle);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Снятие скриншота окна
        /// </summary>
        /// <returns>Скрин или null в случае неудачи</returns>
        public Bitmap CaptureScreen()
        {
            if (Handle == IntPtr.Zero) return null;
            RECT windowRect = new RECT();
            GetWindowRect(Handle, ref windowRect);
            var Left = windowRect.left + config.xOffset;
            var Top = windowRect.top + config.yOffset;

            int h = windowRect.right - Left;
            int w = windowRect.bottom - Top;

            if (h > config.maxHeight) h = config.maxHeight;
            if (w > config.maxWidth) w = config.maxWidth;

            Bitmap BMP = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            System.Drawing.Graphics GFX = System.Drawing.Graphics.FromImage(BMP);
            IntPtr del = GFX.GetHdc();
            IntPtr dc2 = GetWindowDC(Handle);
             //CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt
            BitBlt(del, 0, 0, BMP.Width, BMP.Height, dc2, config.xOffset, config.yOffset, (int)(CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt));
            GFX.ReleaseHdc(del);
            ReleaseDC(Handle, dc2);

            return BMP;
        }
    }
}
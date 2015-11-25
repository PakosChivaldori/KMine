using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace kmine
{
    /// <summary>
    /// Класс для нажатия мышкой
    /// </summary>
    [Keyword("click")]
    public class PressCommand : ICommand
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        const int MOUSEEVENTF_LEFTDOWN = 0x02;
        const int MOUSEEVENTF_LEFTUP = 0x04;

        bool doubleClick;

        public string Name { get; set; }
        public Config Cfg { get; set; }
        public PressCommand(string name, Config cfg) { Name = name; Cfg = cfg; doubleClick = false; }
        public bool Parse(string data) { if (!string.IsNullOrWhiteSpace(data) && data.Equals("double")) doubleClick = true; return true; }
        public void Action(ScriptState state)
        {
            Click(state);
            if (doubleClick)
                Click(state);
            state.ActionDone = true;
        }

        private static void Click(ScriptState state)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, state.Mouse.X, state.Mouse.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, state.Mouse.X, state.Mouse.Y, 0, 0);
        }
    }

    public enum Position { Left, Right, Top, Bottom, Center }

    /// <summary>
    /// Класс для позиционирования мышки
    /// </summary>
    [Keyword("position")]
    public class PositionCommand : ICommand
    {
        public string Name { get; set; }
        public Config Cfg { get; set; }
        public Position Pos { get; private set; }
        public PositionCommand(string name, Config cfg) { Name = name; Cfg = cfg; Pos = Position.Center; }
        public bool Parse(string data)
        {
            if (data.StartsWith(":"))
                data = data.Substring(1).Trim();
            Position pos;
            if (Enum.TryParse(data, true, out pos))
                Pos = pos;
            else
                Program.log.Error("Error position {0}", data);

            return true;
        }

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        public void Action(ScriptState state)
        {
            if (state.Rectangle.IsEmpty) return;
            int X = state.Rectangle.X + Cfg.xOffset, Y = state.Rectangle.Y + Cfg.yOffset;
            int W = state.Rectangle.Width / 2;
            int H = state.Rectangle.Height / 2;
            switch (Pos)
            {
                case Position.Center:
                    X += W;
                    Y += H;
                    break;
                case Position.Left:
                    Y += H;
                    break;
                case Position.Right:
                    Y += H;
                    X += state.Rectangle.Width;
                    break;
                case Position.Top:
                    X += W;
                    break;
                case Position.Bottom:
                    X += W;
                    Y += state.Rectangle.Height;
                    break;
            }
            state.Mouse = new Point(X, Y);
            SetCursorPos(X, Y);
        }
    }

    /// <summary>
    /// Проверка режима (не пускает дальше при несовпадающем)
    /// </summary>
    [Keyword("mode")]
    public class ModeCommand : ICommand
    {
        public string Name { get; set; }
        public Config Cfg { get; set; }
        private string Mode { get; set; }

        public ModeCommand(string name, Config cfg) { Name = name; Cfg = cfg; }
        public bool Parse(string data)
        {
            Mode = data;
            return true;
        }
        public void Action(ScriptState state)
        {
            state.Running = ScriptState.Mode.Equals(Mode);
        }
    }

    /// <summary>
    /// Установка текущего режима
    /// </summary>
    [Keyword("setmode")]
    public class SetModeCommand : ICommand
    {
        public string Name { get; set; }
        public Config Cfg { get; set; }
        private string Mode { get; set; }

        public SetModeCommand(string name, Config cfg) { Name = name; Cfg = cfg; }
        public bool Parse(string data)
        {
            Mode = data;
            return true;
        }
        public void Action(ScriptState state)
        {
            ScriptState.Mode = Mode;
            Program.log.Trace("Set mode {0}",Mode);
        }
    }

    /// <summary>
    /// Совпадение с картинкой, не пускает дальше при несовпадении (с @ начинаются имена встроенных в ресурсы картинок)
    /// </summary>
    [Keyword("match")]
    public class MatchCommand : ICommand
    {
        public string Name { get; set; }
        public Config Cfg { get; set; }
        public string ImageName { get; private set; }
        public GImage Img { get; private set; }

        public MatchCommand(string name, Config cfg) { Name = name; Cfg = cfg; }
        public bool Parse(string data)
        {
            if (data.StartsWith(@"@"))
            {
                var i = data.Remove(0, 1);
                var o = global::kmine.Properties.Resources.ResourceManager.GetObject(i) as Bitmap;
                if (o != null)
                {
                    Img = new GImage(o);
                }
                else
                    Program.log.Error("Can't locate image {0}", data);
            }
            else
                if ((Img = Cfg[data]) == null)
                    Program.log.Error("Can't locate image {0}", data);
            return true;
        }
        public void Action(ScriptState state)
        {
            if (Img != null)
            {
                // Ищем совпадения
                var p = state.Image.Find(Img, new Point());
                if (p.IsEmpty)
                    state.Running = false;
                else
                    state.Rectangle = new Rectangle(p.X, p.Y, Img.Width, Img.Height);
            }
            else state.Running = false;
        }
    }
}

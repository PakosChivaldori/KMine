using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using NLog.Targets;

namespace kmine
{
    public partial class MainForm : Form
    {
        Config config = new Config();
        Commands commands = null;
        Script[] scripts = null;
        GImage oldbmp = null;
        bool action = true;

        enum Mode { None, Wait, Active };

        Mode _mode = Mode.None;

        Mode mode
        {
            get { return _mode; }
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    switch (value)
                    {
                        case Mode.Wait:
                            ind.Image = global::kmine.Properties.Resources.clock;
                            break;
                        case Mode.Active:
                            ind.Image = global::kmine.Properties.Resources.greenlight;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public MainForm()
        {
            InitializeComponent();
            kmine.WindowTarget.RegisterWindow(this);
            commands = new Commands(config);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Screen rightmost = Screen.AllScreens[0];
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.Right > rightmost.WorkingArea.Right)
                    rightmost = screen;
            }

            this.Left = rightmost.WorkingArea.Right - this.Width;

            scripts = Script.LoadScript(config, commands);
            if (GImage.FindProper(config))
            {
                tm.Enabled = true;
            }
            else Program.log.Error("No window found");
        }

        private void tm_Tick(object sender, EventArgs e)
        {
            var wbmp = GImage.Capture(config);
            pb1.Image = wbmp;
            //x.Save("1.bmp");
            if (wbmp != null)
            {
                var bmp = new GImage(wbmp);
                bool ok = false;
                // Сравиваем экран с предыдущим
                if (oldbmp != null)
                {
                    if (bmp.Equals(oldbmp))
                    {
                        ok = true;
                        mode = Mode.Active;
                    }
                    else mode = Mode.Wait;

                }
                // Если ок - смотрим на экшн, если не ок - значит началось и можно взводить ожидание экшна после успокоения
                // !!!! сделать таймер, который взводит экшн даже когда всё оставалось ок
                if (ok)
                {
                    if (action)
                    {
                        // Прогоняем все скрипты
                        foreach (var im in scripts)
                        {
                            if (im.Run(bmp))
                            {
                                action = false;
                                grid.DrawField();
                            }
                        }
                        CMode.Text = ScriptState.Mode;
                    }
                }
                else
                {
                    action = true;
                }
                oldbmp = bmp;
            }
            else Close();
        }
        public void Log(string s, LogLevel level)
        {
            var box = level == LogLevel.Debug || level == LogLevel.Info || level == LogLevel.Trace ? infoBox : logBox;
            while (box.Items.Count > 8)
                box.Items.RemoveAt(0);
            box.Items.Add(s);
        }
    }

    [Target("Window")]
    public class WindowTarget : TargetWithLayout
    {
        static MainForm form = null;

        protected override void Write(LogEventInfo logEvent)
        {
            if (form != null)
                form.Log(logEvent.Parameters == null ? logEvent.Message : string.Format(logEvent.Message, logEvent.Parameters), logEvent.Level);
        }

        public static void RegisterWindow(MainForm window)
        {
            form = window;
        }
    }
}
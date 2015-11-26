using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace kmine
{
    public class Script
    {
        static Commands commandList = null;

        /// <summary>
        /// Имя команды скрипта
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Распарсенный список команд скрипта
        /// </summary>
        public List<ICommand> commands { get; set; }

        public Script(string fn)
        {
            Name = fn;
            commands = new List<ICommand>();
        }

        /// <summary>
        /// Распарсить очередную строку
        /// </summary>
        /// <param name="s">Входная строка</param>
        public void Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return;
            var ts = s.Trim();

            var com = ts.Split(new char[] { ' ', ',', ':', '#', '=' });
            var c = commandList.Command(com[0]);
            if (c != null)
            {
                if (c.Parse(ts.Substring(com[0].Length).Trim()))
                    commands.Add(c);
                else
                    Program.log.Error("Error parsing command {0}", s);
            }
            else
                Program.log.Error("Unknown command {0}", s);
        }

        /// <summary>
        /// Инициализация скриптового движка
        /// </summary>
        /// <param name="commands">Ссылка на список команд</param>
        public static void InitScript(Commands commands)
        {
            commandList = commands;
        }

        /// <summary>
        /// Загрузить скрипты из файла
        /// </summary>
        /// <param name="cfg">Ссылка на объект конфигурации</param>
        /// <param name="commands">Ссылка на список команд</param>
        /// <returns>Список скриптов</returns>
        public static Script[] LoadScript(Config cfg, Commands commands)
        {
            InitScript(commands);
            var scripts = new List<Script>();
            var a = System.IO.File.ReadAllText(@"mine.script").Split('\n');
            Script current = null;
            foreach (var s in a)
            {
                if (!string.IsNullOrWhiteSpace(s))
                {
                    var st = s.Trim();
                    if (!(st.StartsWith("#") || st.StartsWith("//")))
                    {
                        // Начало блока скрипта с переменной
                        if (st.StartsWith("@"))
                        {
                            var p = st.IndexOf(':');
                            if (p < 0)
                                Program.log.Error("Script name definition error {0}", s);
                            else
                            {
                                var sn = st.Substring(1, p - 1).Trim();
                                if (!scripts.Exists(m => m.Name == sn))
                                {
                                    current = new Script(sn);
                                    current.Parse(st.Substring(p + 1));
                                    scripts.Add(current);
                                }
                                else
                                {
                                    Program.log.Error("Duplicate script {0}", s);
                                }
                            }
                        }
                        else if (current == null)
                        {
                            // До первого скрипта - блок глобальных параметров
                            cfg.ReadConfig(st);
                        }
                        else
                            // После заголовка скрипта всё относится к нему
                            current.Parse(s);
                    }
                }
            }
            return scripts.ToArray();
        }

        /// <summary>
        /// Запуск скрипта
        /// </summary>
        /// <param name="img">снятое изображение экрана</param>
        /// <returns>Скрипт выполнил задачу (false - отработал вхолостую)</returns>
        internal bool Run(GImage img)
        {
            Program.log.Info(Name);
            var state = new ScriptState(this, img);
            foreach (var c in commands)
            {
                Program.log.Info(" {0}", c.Name);
                c.Action(state);
                if (!state.Running)
                    break;
            }
            return state.ActionDone;
        }
    }

    /// <summary>
    /// Описание скриптовой команды
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Имя
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Ссылка на конфигурацию
        /// </summary>
        Config Cfg { get; }
        /// <summary>
        /// Действие
        /// </summary>
        /// <param name="state">Ссылка на состояние</param>
        void Action(ScriptState state);
        /// <summary>
        /// Парсинг
        /// </summary>
        /// <param name="data">параметры (опционально)</param>
        /// <returns>Нет ошибок парсинга</returns>
        bool Parse(string data);
    }

    /// <summary>
    /// Состояние выполнения скрипта
    /// </summary>
    public class ScriptState
    {
        static Size fieldSize;

        // Текущий режим
        public static string Mode { get; set; }
        // Координаты поля
        public static Rectangle FieldRectangle { get; set; }
        // Размеры поля
        public static Size FieldSize
        {
            get { return fieldSize; }
            set { var flag = !fieldSize.Equals(value); if (flag) { fieldSize = value; SizeChanged(); } }
        }
        // Имя шахты
        public static string Shaft { get; set; }

        // Количество М.
        public static int TotalMines { get; set; }
        // Список ячеек
        static List<GridCell> Cells;
        // Осталось М.
        static int CurrentMines { get; set; }

        // Сслыка на выполняемый скрипт
        public Script Parent { get; private set; }
        // Работыет ли текущий скрипт
        public bool Running { get; set; }
        // Ссылка на текущий образ экрана
        public GImage Image { get; private set; }
        // Был выполнен экшн, нужна пауза пока поступит реакция
        public bool ActionDone { get; set; }
        // Прямоугольник найденной области
        public Rectangle Rectangle { get; set; }
        // Мышиная позиция
        public Point Mouse { get; set; }

        public ScriptState(Script script, GImage image)
        {
            Parent = script;
            Running = true;
            Image = image;
            ActionDone = false;
            Rectangle = new Rectangle();
        }

        static ScriptState()
        {
            Mode = string.Empty;
            FieldRectangle = new Rectangle();
            FieldSize = new Size();
            TotalMines = 0;
            Cells = null;
        }

        /// <summary>
        /// Осталось М.
        /// </summary>
        public int OpenedMines
        {
            get
            {
                var m = TotalMines;
                if (Cells != null)
                    foreach (var c in Cells)
                        if (c.Flag) --m;
                return m;
            }
        }

        public GridCell this[int x, int y]
        {
            get { return GetGridCell(x, y); }
        }

        public static GridCell GetGridCell(int x, int y)
        {
            return Cells == null || x >= FieldSize.Width || y >= FieldSize.Height ? null : Cells[x + y * FieldSize.Width];
        }

        private static void SizeChanged()
        {
            var s = fieldSize.Width * fieldSize.Height;
            Cells = new List<GridCell>(s);
            while (s-- > 0)
                Cells.Add(new GridCell());
        }
        /// <summary>
        /// Список свободных ячеек
        /// </summary>
        public List<GridCell> FreeCells()
        {
            var l = new List<GridCell>();
            foreach (var c in Cells)
                if (c.NotOpened) l.Add(c);
            return l;
        }
    }
}
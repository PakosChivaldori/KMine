using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace kmine_bump
{
    /// <summary>
    /// Скрипт выкладки
    /// </summary>
    public class Script
    {
        /// <summary>
        /// Обозначение встроенной переменной для запоминания результата команды cd
        /// </summary>
        public const string WorkingDirectory = "WorkingDirectory";
        public const string UpdatedVariable = "Updated";
        public const string LabelName = "label";
        static Dictionary<string, Type> commands;
        public Variables variables { get; private set; }
        List<ICommand> script { get; set; }
        public string WaitingLabel { get; set; }
        public bool Skip { get; set; }
        public Dictionary<string, ICommand> labels { get; private set; }

        // Конструируем список возможных команд
        static Script()
        {
            commands = new Dictionary<string, Type>();
            // Считаем что они все лежат в нашей сборке
            // MEF не применяем, т.к. нам нужен список типов, а не экземпляров (экземпляр сделаем по требованию)
            Type[] types = typeof(Script).Assembly.GetTypes();
            foreach (var t in types)
            {
                if (t.GetInterface("ICommand") == typeof(ICommand))
                {
                    // Получаем все ICommand
                    var attr = System.Attribute.GetCustomAttributes(t, false).Where(a => a.GetType() == typeof(KeywordAttribute));
                    foreach (var a in attr)
                    {
                        var name = ((KeywordAttribute)a).Name;
                        if (!commands.ContainsKey(name))
                            commands.Add(name, t);
                    }
                }
            }
        }

        /// <summary>
        /// Экземпляр скрипта
        /// </summary>
        public Script()
        {
            variables = new Variables();
            script = new List<ICommand>();
            labels = new Dictionary<string, ICommand>();
        }

        /// <summary>
        /// Построчный парсинг скрипта
        /// </summary>
        /// <param name="s">Строка исходника</param>
        public void Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return;
            var ts = s.Trim();

            if (ts.StartsWith("#") || ts.StartsWith("//"))
                return;

            if (ts.EndsWith(":"))
                ts = string.Format("{0} {1}", Script.LabelName, ts.Substring(0, ts.Length - 1).Trim());
            var com = ts.Split(new char[] { ' ', ',', ':', '#', '=' });
            // Находим команду и пусть сама себя парсит
            var c = this[com[0]];
            if (c != null)
            {
                if (c.Parse(ts.Substring(com[0].Length).Trim()))
                    script.Add(c);
                else
                    Message("Error parsing command {0}", s);
            }
            else
                Message("Unknown command {0}", s);
        }

        /// <summary>
        /// Запуск скрипта
        /// </summary>
        public void Run()
        {
            WaitingLabel = null;
            Skip = false;
            foreach (var c in script)
            {
                // Если не пропуск - работаем
                if (!Skip)
                    c.Action();
                // Если пропуск - проверяем наличие целевой метки и наличие её на текущем шаге
                else if (WaitingLabel != null && c == labels[WaitingLabel])
                    Skip = false;
            }
        }

        public void Message(string m)
        {
            Console.WriteLine(m);
        }

        public void Message(string m, params object[] data)
        {
            Console.WriteLine(m, data);
        }

        /// <summary>
        /// Дай экземпляр команды по имени
        /// </summary>
        public ICommand Command(string name)
        {
            if (commands.ContainsKey(name))
            {
                var ic = (ICommand)commands[name].InvokeMember(null, BindingFlags.DeclaredOnly |
    BindingFlags.Public |
    BindingFlags.Instance | BindingFlags.CreateInstance, null, null, new object[] { name, this });
                return ic;
            }
            else return null;
        }

        public ICommand this[string name] { get { return Command(name); } }
    }

    /// <summary>
    /// Прототип команды
    /// </summary>
    public interface ICommand
    {
        bool Parse(string data);
        void Action();
    }

    /// <summary>
    /// Переменные скрипта
    /// </summary>
    public class Variables
    {
        Dictionary<string, string> vars;

        public Variables()
        {
            vars = new Dictionary<string, string>();
        }

        public string this[string name]
        {
            get { return vars.ContainsKey(name) ? vars[name] : string.Empty; }
            set
            {
                // Переменная может конструироваться из других переменных
                vars[name] = Prepare(value);
            }
        }

        /// <summary>
        /// Парсинг строки с подстановкой переменных
        /// </summary>
        public string Prepare(string value)
        {
            int n = 0;
            // $ - символ подстановки
            while (value.Substring(n).Contains(@"$"))
            {
                var p = value.Substring(n).IndexOf('$');
                if (value.Length > n + p + 1)
                {
                    if (value[n + p + 1] == '$')
                    {
                        value = value.Remove(n + p, 1);
                        n += p + 1;
                    }
                    else
                    {
                        n += p;
                        p = 1;
                        while (value.Length > n + p && ((char.IsLetter(value, n + p) || char.IsDigit(value, n + p))))
                        {
                            ++p;
                        }
                        if (p > 1)
                        {
                            var tmp = value.Substring(n + 1, p - 1);
                            var tv = this[tmp];
                            value = (n > 0 ? value.Substring(0, n) : string.Empty) + tv +
                                (value.Length > n + p ? value.Substring(n + p) : string.Empty);
                            n += tv.Length;
                        }
                        else ++n;
                    }
                }
                else n = value.Length;
            }
            return value;
        }
    }

    /// <summary>
    /// <b>set</b> <i>variable</i> <i>value</i>
    ///  <i>variable</i> - имя или <i>function</i><b>-></b><i>name</i>
    ///  function:
    ///   ver - dll/exe version
    ///   date - file modification date
    ///   datever - version from date YYYYMMDDhhmm
    ///   size - размер файла
    ///   updated - были ли обновлены файлы последними командами обновления
    ///  <i>value</i> может содержать $variable (уже объявленные)
    /// </summary>
    [Keyword("set")]
    public class SetCommand : ICommand
    {
        string Name { get; set; }
        Script Parent { get; set; }
        string Data { get; set; }

        public SetCommand(string name, Script parent) { Name = name; Parent = parent; }
        public bool Parse(string data)
        {
            Data = data;
            return true;
        }
        public void Action()
        {
            // Устанавливаем значение переменной
            var tmp = Data.Split(new char[] { ' ' }, 2);
            if (tmp[0].Contains("->"))
            {
                var what = tmp[0].Substring(0, tmp[0].IndexOf("->"));
                tmp[0] = tmp[0].Remove(0, what.Length + 2);
                var fn = tmp.Length > 1 ? Parent.variables.Prepare(tmp[1]) : string.Empty;
                switch (what)
                {
                    case "ver":
                        tmp[1] = Ver(fn);
                        break;
                    case "datever":
                        tmp[1] = Date(fn, "yyyyMMddHH");
                        break;
                    case "date":
                        tmp[1] = Date(fn, "g");
                        break;
                    case "size":
                        tmp[1] = Size(fn);
                        break;
                    case "updated":
                        // Значение сохраняем и обнуляем
                        var p = Parent.variables[Script.UpdatedVariable];
                        Parent.variables[Script.UpdatedVariable] = string.Empty;
                        if (tmp.Length > 1) tmp[1] = p; else tmp = new[] { tmp[0], p };
                        break;
                    default:
                        tmp[1] = string.Format(@"Error function: {0}", what);
                        break;
                }
            }
            Parent.variables[tmp[0]] = tmp[1].Trim();
        }

        private string Size(string fn)
        {
            var i = new FileInfo(fn).Length;
            if (i < 1024)
                return string.Format(@"{0}", i);
            if (i < 1024 * 1024)
                return string.Format(@"{0}K", i / 1024);
            return ((double)i / 1024 / 1024).ToString(",0.0M", CultureInfo.InvariantCulture);
        }

        private string Date(string fn, string fmt)
        {
            var i = new FileInfo(fn);
            return i.LastWriteTime.ToString(fmt);
        }

        private string Ver(string fn)
        {
            return FileVersionInfo.GetVersionInfo(fn).FileVersion;
        }
    }

    /// <summary>
    /// <b>echo</b> <i>message</i>
    /// </summary>
    [Keyword("echo")]
    public class EchoCommand : ICommand
    {
        string Name { get; set; }
        Script Parent { get; set; }
        string Data { get; set; }

        public EchoCommand(string name, Script parent) { Name = name; Parent = parent; }
        public bool Parse(string data)
        {
            Data = data;
            return true;
        }
        public void Action()
        {
            Parent.Message(Parent.variables.Prepare(Data));
        }
    }

    /// <summary>
    /// <b>execute</b> <i>command</i>
    /// </summary>
    [Keyword("execute")]
    public class ExecuteCommand : ICommand
    {
        string Name { get; set; }
        Script Parent { get; set; }
        string Data { get; set; }

        public ExecuteCommand(string name, Script parent) { Name = name; Parent = parent; }
        public bool Parse(string data)
        {
            Data = data;
            return true;
        }
        public void Action()
        {
            var exe = Parent.variables.Prepare(Data);

            var tmp = exe.Split(' ');

            var procStartInfo = new ProcessStartInfo(tmp[0], exe.Remove(0, tmp[0].Length + 1).Trim());
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;
            var wd = Parent.variables[Script.WorkingDirectory];
            if (!string.IsNullOrEmpty(wd))
                procStartInfo.WorkingDirectory = wd;
            var proc = new Process();
            proc.StartInfo = procStartInfo;
            try
            {
                proc.Start();
                var result = proc.StandardOutput.ReadToEnd();
                Parent.Message("Execute({1}): {0}", exe, proc.ExitCode);
                if (proc.ExitCode != 0)
                    foreach (var s in result.Split('\n'))
                        Parent.Message("  {0}", s.Replace('\r', ' '));
            }
            catch (Exception e)
            {
                Parent.Message(@"Exception on execute {0}: {1}", exe, e.Message);
            }

        }
    }

    /// <summary>
    /// <b>remove</b> <i>path</i> <i>mask</i>
    /// </summary>
    [Keyword("remove")]
    public class RemoveCommand : ICommand
    {
        string Name { get; set; }
        Script Parent { get; set; }
        string Data { get; set; }

        public RemoveCommand(string name, Script parent) { Name = name; Parent = parent; }
        public bool Parse(string data)
        {
            Data = data;
            return true;
        }
        public void Action()
        {
            var tmp = Data.Split(' ');
            if (tmp.Length != 2)
                Parent.Message("Error: wrong remove arg count in {0}: {1}", Data, tmp.Length);
            else
            {
                // Удаляем файлы по списку
                try
                {
                    var fl = Directory.GetFiles(Parent.variables.Prepare(tmp[0]), Parent.variables.Prepare(tmp[1]));
                    foreach (var f in fl)
                        File.Delete(f);
                }
                catch (Exception e)
                {
                    Parent.Message(@"Exception on remove {0}", e.Message);
                }
            }
        }
    }

    /// <summary>
    /// set working directory for execute
    /// <b>cd</b> <i>path</i>
    /// </summary>
    [Keyword("cd")]
    public class CDCommand : ICommand
    {
        string Name { get; set; }
        Script Parent { get; set; }
        string Data { get; set; }

        public CDCommand(string name, Script parent) { Name = name; Parent = parent; }
        public bool Parse(string data)
        {
            Data = data;
            return true;
        }
        public void Action()
        {
            Parent.variables[Script.WorkingDirectory] = Data;
        }
    }

    /// <summary>
    /// update file command
    /// <b>update</b> <i>file</i> <i>src-path</i> <i>dst-path</i>
    /// </summary>
    [Keyword("update")]
    public class UpdateCommand : ICommand
    {
        string Name { get; set; }
        Script Parent { get; set; }
        string Data { get; set; }

        public UpdateCommand(string name, Script parent) { Name = name; Parent = parent; }
        public bool Parse(string data)
        {
            Data = data;
            return true;
        }
        string Hash(string fn)
        {
            var md5 = MD5.Create();
            md5.Initialize();
            using (var f = new FileStream(fn, FileMode.Open))
            {
                var h = md5.ComputeHash(f);
                f.Close();
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < h.Length; i++)
                {
                    sBuilder.Append(h[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }
        public void Action()
        {
            var tmp = Data.Split(' ');
            if (tmp.Length != 3)
                Parent.Message("Error: wrong update arg count in {0}: {1}", Data, tmp.Length);
            {
                var obj = Parent.variables.Prepare(tmp[0]);
                var sd = Parent.variables.Prepare(tmp[1]);
                var src = Path.Combine(sd, obj);
                var dd = Parent.variables.Prepare(tmp[2]);
                var dst = Path.Combine(dd, obj);
                // Если есть wildcards - получаем список и копируем по одному
                if (obj.Contains('?') || obj.Contains('*'))
                {
                    var dir = Directory.GetFiles(sd, obj);
                    foreach (var d in dir)
                    {
                        CopyFile(d, Path.Combine(dd, Path.GetFileName(d)));
                    }
                }
                else
                    CopyFile(src, dst);
            }
        }

        private void CopyFile(string src, string dst)
        {
            if (File.Exists(src))
            {
                var ok = true;
                if (File.Exists(dst))
                {
                    var s = Hash(src);
                    if (s.Equals(Hash(dst)))
                        ok = false;
                }
                if (ok)
                    try
                    {
                        Parent.Message(@"Copying {0} to {1}", src, dst);
                        File.Copy(src, dst, true);
                        Parent.variables[Script.UpdatedVariable] = "true";
                    }
                    catch (Exception e)
                    {
                        Parent.Message(@"Exception on copy {0} to {1}: {2}", src, dst, e.Message);
                    }
            }
            else
                Parent.Message(@"File doesn't exists in {0}", src);
        }
    }
    /// <summary>
    /// Создание файла по шаблону
    /// <b>template</b> <i>template-file</i> <i>output-file</i>
    /// </summary>
    [Keyword("template")]
    public class TemplateCommand : ICommand
    {
        string Name { get; set; }
        Script Parent { get; set; }
        string Data { get; set; }

        public TemplateCommand(string name, Script parent) { Name = name; Parent = parent; }
        public bool Parse(string data)
        {
            Data = data;
            return true;
        }
        public void Action()
        {
            var tmp = Data.Split(' ');
            if (tmp.Length != 2)
                Parent.Message("Error: wrong template arg count in {0}: {1}", Data, tmp.Length);
            else
            {
                try
                {
                    var q = File.ReadAllLines(Parent.variables.Prepare(tmp[0]));
                    for (int i = 0; i < q.Length; ++i)
                        q[i] = Parent.variables.Prepare(q[i]);
                    File.WriteAllLines(Parent.variables.Prepare(tmp[1]), q);
                }
                catch (Exception e)
                {
                    Parent.Message(@"Exception on template {0}: {1}", tmp[0], e.Message);
                }

            }
        }
    }

    /// <summary>
    /// Метка
    /// </summary>
    [Keyword(Script.LabelName)]
    public class LabelCommand : ICommand
    {
        string Name { get; set; }
        Script Parent { get; set; }
        string Data { get; set; }

        public LabelCommand(string name, Script parent) { Name = name; Parent = parent; }
        public bool Parse(string data)
        {
            Data = data;
            Parent.labels.Add(Data, this);
            return true;
        }
        public void Action() { }
    }

    /// <summary>
    /// Условный оператор
    /// </summary>
    [Keyword("if")]
    public class IfCommand : ICommand
    {
        enum Mode { Exists, Variable };
        enum Act { Stop, Goto };

        string Name { get; set; }
        Script Parent { get; set; }
        string Data { get; set; }
        bool Not;
        Mode mode;
        string Param;
        Act act;

        public IfCommand(string name, Script parent) { Name = name; Parent = parent; Not = false; }
        public bool Parse(string data)
        {
            if (data.StartsWith("!"))
            {
                Not = true;
                data = data.Remove(0, 1).Trim();
            }
            var arr = data.Split(' ');
            var m = 0;
            foreach (var a in arr)
            {
                switch (m)
                {
                    case 0:
                        if (a.StartsWith("$"))
                        {
                            mode = Mode.Variable;
                            Param = a.Substring(1);
                            m = 2;
                        }
                        else if (a.Equals("exists"))
                        {
                            mode = Mode.Exists;
                            m = 1;
                        }
                        break;
                    case 1:
                        if (!string.IsNullOrWhiteSpace(a))
                        {
                            Param = a;
                            m = 2;
                        }
                        break;
                    case 2:
                        switch (a)
                        {
                            case "stop":
                                act = Act.Stop;
                                m = 4;
                                break;
                            case "goto":
                                act = Act.Goto;
                                m = 3;
                                break;
                        }
                        break;
                    case 3:
                        if (!string.IsNullOrWhiteSpace(a))
                        {
                            Data = a;
                            m = 4;
                        }
                        break;
                }
            }

            if (m != 4)
            {
                Parent.Message("Error: if construction fails with {0} tags", m);
                return false;
            }
            return true;
        }
        public void Action()
        {
            var cond = Not;
            switch (mode)
            {
                case Mode.Exists:
                    cond = File.Exists(Parent.variables.Prepare(Param));
                    break;
                case Mode.Variable:
                    cond = !string.IsNullOrWhiteSpace(Parent.variables[Param]);
                    break;
            }
            if (Not) cond = !cond;
            switch (act)
            {
                case Act.Stop:
                    // Просто отрабатывает вхолостую
                    if (cond) Parent.Skip = true;
                    break;
                case Act.Goto:
                    // Пропускаем все шаги и выставляем метку, с которой должно снова заработать
                    if (cond) { Parent.Skip = true; Parent.WaitingLabel = Data; }
                    break;
            }
        }
    }

    /// <summary>
    /// Имя команды скрипта
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class KeywordAttribute : Attribute
    {
        public string Name { get; private set; }
        public KeywordAttribute(string name) { Name = name; }
    }
}
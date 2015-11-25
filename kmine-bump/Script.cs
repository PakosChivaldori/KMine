using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace kmine_bump
{
    public class Script
    {
        static Dictionary<string, Type> commands;
        public Variables variables { get; private set; }
        List<ICommand> script { get; set; }

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
                    // Получаем все ContractAttribute 
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

        public Script()
        {
            variables = new Variables();
            script = new List<ICommand>();
        }

        public void Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return;
            var ts = s.Trim();

            var com = ts.Split(new char[] { ' ', ',', ':', '#', '=' });
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

        public void Run()
        {
            foreach (var c in script)
                c.Action();
        }

        public void Message(string m)
        {
            Console.WriteLine(m);
        }

        public void Message(string m, params object[] data)
        {
            Console.WriteLine(m, data);
        }

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

    public interface ICommand
    {
        bool Parse(string data);
        void Action();
    }

    public class Variables
    {
        Dictionary<string, string> vars;

        public Variables()
        {
            vars = new Dictionary<string, string>();
        }

        public string this[string name]
        {
            // Prepare на чтении, т.к. двойные $$ могут быть и после записи это окажется именем переменной
            get { return vars.ContainsKey(name) ? Prepare(vars[name]) : string.Empty; }
            set
            {
                vars[name] = value;
            }
        }

        public string Prepare(string value)
        {
            int n=0;
            while (value.Substring(n).Contains(@"$"))
            {
                var p = value.Substring(n).IndexOf('$');
                if (value.Length > n + p)
                {
                    if (value[n + p + 1] == '$')
                    {
                        value = value.Remove(n + p, 1);
                        n += p + 1;
                    }
                    else
                    {
                        ///!!!! поиск имени переменной для подстановки и замена её с пересчётом длины сообщения
                        ///// и повтор с новой позиции
                        // юнит-тесты для проверки
                    }
                }
                else n = value.Length;
            }
            return value;
            /// !!!! преобразовывать с учётом макросов
        }
    }

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
            var tmp = Data.Split(new char[] { ' ' }, 2);
            Parent.variables[tmp[0]] = tmp[1].Trim();
        }
    }

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

    [AttributeUsage(AttributeTargets.Class)]
    public class KeywordAttribute : Attribute
    {
        public string Name { get; private set; }
        public KeywordAttribute(string name) { Name = name; }
    }

}
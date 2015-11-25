using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace kmine
{
    /// <summary>
    /// Список доступных команд для скриптов
    /// </summary>
   public class Commands
    {
        Dictionary<string, Type> commands;
        Config config;

        public Commands(Config cfg)
        {
            config = cfg;
            commands = new Dictionary<string, Type>();
            // Считаем что они все лежат в нашей сборке
            // MEF не применяем, т.к. нам нужен список типов, а не экземпляров (экземпляр сделаем по требованию)
            Type[] types = this.GetType().Assembly.GetTypes();
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
       /// <summary>
       /// Получить экземнпляр команды по имени
       /// </summary>
       /// <param name="name">имя команды</param>
       /// <returns>Экземпляр</returns>
        public ICommand Command(string name)
        {
            if (commands.ContainsKey(name))
            {
                var ic = (ICommand)commands[name].InvokeMember(null, BindingFlags.DeclaredOnly |
    BindingFlags.Public |
    BindingFlags.Instance | BindingFlags.CreateInstance, null, null, new object[] { name, config });
                return ic;
            }
            else return null;
        }

        public ICommand this[string name] { get { return Command(name); } }
    }

    /// <summary>
    /// Аттрибут для указания имени команды в скрипте (отличается от имени класса)
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
   public class KeywordAttribute : Attribute
   {
       public string Name { get; private set; }
       public KeywordAttribute(string name) { Name = name; }
   }
}
using System;
using System.Drawing;
using kmine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KMineTest
{
    [TestClass]
    public class ScriptTest
    {
        [TestMethod]
        public void ScriptTestMethod()
        {
            var config = new TestConfig();
            Commands commands = new Commands(config);
            config.ReadConfig(new string[] {
                "x-offset=10",
                "y-offset=50",
                "image=firstpic:data\\hint.png"
            });
            Script.InitScript(commands);
            GImage img = new GImage("data\\testsource.png");
            Script s = new Script("test");
            // Грузим исходную картинку, на которой осуществляется поиск
            s.Parse("match firstpic");
            {
                ScriptState st = new ScriptState(s, img);
                Run(s, st);
                Assert.IsTrue(st.Rectangle.Equals(new Rectangle(1238, 853, 196, 37)), "Не найден прямоугольник");
            }
            s = new Script("test");
            s.Parse("setmode first");
            s.Parse("mode first");
            {
                ScriptState st = new ScriptState(s, img);
                Run(s, st);
                Assert.IsTrue(st.Running, "Тест не продолжился");
            }
            s.Parse("mode nfirst");
            {
                ScriptState st = new ScriptState(s, img);
                Run(s, st);
                Assert.IsFalse(st.Running, "Тест продолжился ошибочно");
            }
        }
        private static void Run(Script s, ScriptState st)
        {
            foreach (var c in s.commands)
            {
                //Program.Info(" {0}", ConsoleColor.DarkGray, c.Name);
                c.Action(st);
                if (!st.Running)
                    break;
            }
        }
    }
}

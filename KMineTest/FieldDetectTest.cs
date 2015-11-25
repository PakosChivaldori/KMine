using System;
using System.Drawing;
using kmine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KMineTest
{
    [TestClass]
    public class FieldDetectTest
    {
        [TestMethod]
        public void DetectField()
        {
            var config = new TestConfig();
            Commands commands = new Commands(config);
            config.ReadConfig(new string[] {
                "x-offset=10",
                "y-offset=50",
            });
            Script.InitScript(commands);
            GImage img = new GImage("data\\fullfield.png");
            Script s = new Script("test");
            // Грузим исходную картинку, на которой осуществляется поиск
            s.Parse("detectfield");
            s.Parse("fielddetected");
            var nimg = new GImage("data\\nofield.png");
            {
                ScriptState st = new ScriptState(s, nimg);
                Run(s, st);
                Assert.IsFalse(st.Running, "Лишнее поле нашлось");
            }
            {
                ScriptState st = new ScriptState(s, img);
                Run(s, st);
                Assert.IsTrue(st.Running, "Поле не нашлось");
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
        [TestMethod]
        public void TM()
        {
            var config = new TestConfig();
            Commands commands = new Commands(config);
            config.ReadConfig(new string[] {
                "x-offset=10",
                "y-offset=50",
            });
            Script.InitScript(commands);
            var img = new GImage("src.bmp");
            Script s = new Script("test");
            // Грузим исходную картинку, на которой осуществляется поиск
            s.Parse("detectfield");
            s.Parse("fielddetected");
            s.Parse("fieldcells");
            {
                ScriptState st = new ScriptState(s, img);
                Run(s, st);
            }
        }
    }
}

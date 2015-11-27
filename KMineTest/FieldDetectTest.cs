using System;
using System.Drawing;
using System.IO;
using System.Text;
using Emgu.CV.Structure;
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
            var img = new GImage(@"data\leofield.jpg");
            Script s = new Script("test");
            // Грузим исходную картинку, на которой осуществляется поиск
            s.Parse("detectfield");
            s.Parse("fielddetected");
            s.Parse("fieldcells");
            {
                ScriptState st = new ScriptState(s, img);
                Run(s, st);
                StringBuilder sb = new StringBuilder(1024);
                for (var y = 0; y < ScriptState.FieldSize.Height; ++y)
                {
                    for (var x = 0; x < ScriptState.FieldSize.Width; ++x)
                    {
                        var c = st[x, y];
                        var t = c.NotOpened ? '_' : c.Flag ? 'P' : c.Empty ? ' ' : (char)((byte)'0' + c.Number);
                        sb.Append(t);
                        sb.Append(' ');
                    }
                    sb.Append("\n");
                }
                File.WriteAllText("out.txt", sb.ToString());
            }
        }
        [TestMethod]
        public void FlashDetectTest()
        {
            var config = new TestConfig();
            Commands commands = new Commands(config);
            config.ReadConfig(new string[] {
                "x-offset=702",
                "y-offset=30",
                "max-width=1000",
                "max-height=800"
            });
            var img = new Bitmap("data\\nofield.png");
            GImage.FindFlash(config, true, true, img);
            var r = new GImage(img);
            r.Draw(new Rectangle(config.xOffset, config.yOffset, config.maxWidth, config.maxHeight), new Gray(128), 2);
            Assert.AreEqual(832, config.xOffset, "Detect X");
            Assert.AreEqual(130, config.yOffset, "Detect Y");
            // r.Save("0.bmp");
        }
    }
}
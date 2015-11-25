using System;
using kmine_bump;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BumpTest
{
    [TestClass]
    public class ScriptTest
    {
        [TestMethod]
        public void IfTest()
        {
            var s = new Script();
            s.Parse("set true 1");
            s.Parse("if $true stop");
            s.Parse("set error 1");
            s.Run();
            Assert.AreEqual("1", s.variables["true"]);
            Assert.AreEqual(string.Empty, s.variables["error"]);
            s = new Script();
            // true=1
            // if !true goto x (false)
            // stage1=1
            // x:
            // if !false goto z (true)
            // error=1 (don't)
            // z:
            // stage2=1
            s.Parse("set true 1");
            s.Parse("if ! $true goto x");
            s.Parse("set stage1 1");
            s.Parse("x:");
            s.Parse("if ! $false goto z");
            s.Parse("set error 1");
            s.Parse("z:");
            s.Parse("set stage2 1");
            s.Run();
            Assert.AreEqual("1", s.variables["true"]);
            Assert.AreEqual("1", s.variables["stage1"]);
            Assert.AreEqual("1", s.variables["stage2"]);
            Assert.AreEqual(string.Empty, s.variables["error"]);
        }
    }
}

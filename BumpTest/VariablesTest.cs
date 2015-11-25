using System;
using System.Collections.Generic;
using kmine_bump;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BumpTest
{
    [TestClass]
    public class VariablesTest
    {
        [TestMethod]
        public void VarTest()
        {
            const string src = "src";
            const string dst = "dst";
            const string srcT = "Source";
            const string dstT = "Destionation";
            const string equ = "equ";
            var dict = new Dictionary<string, string>();
            dict.Add("src$$", "src");

            Variables v = new Variables();
            v[dst] = dstT;
            v[src] = srcT;
            Assert.AreEqual(v[src], srcT);
            Assert.AreEqual(v[dst], dstT);
            foreach (var t in dict)
            {
                v[equ] = t.Key;
                Assert.AreEqual(v[equ], t.Value);
            }
        }
    }
}
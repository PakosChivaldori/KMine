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
        [Timeout(9999)]
        public void VarTest()
        {
            const string src = "src";
            const string dst = "dst";
            const string srcT = "Source";
            const string dstT = "Destionation";
            const string equ = "equ";
            const string X = "X";
            const string XT = "XXX";
            var dict = new Dictionary<string, string>();
            // Просто спецсимволы
            dict.Add("src$", "src$");
            dict.Add("src$$", "src$");
            dict.Add("$$src", "$src");
            // Включение переменных
            dict.Add("$src", srcT);
            dict.Add("-$src", string.Format(@"-{0}", srcT));
            dict.Add("$src-", string.Format(@"{0}-", srcT));
            dict.Add("-$src-", string.Format(@"-{0}-", srcT));
            dict.Add("$X", XT);
            dict.Add("-$X", string.Format(@"-{0}", XT));
            dict.Add("$X-", string.Format(@"{0}-", XT));
            dict.Add("-$X-", string.Format(@"-{0}-", XT));
            dict.Add("$X$src", string.Format(@"{0}{1}", XT, srcT));
            // Ошибки тоже должны не ронять
            dict.Add("$-", "$-");

            Variables v = new Variables();
            v[dst] = dstT;
            v[src] = srcT;
            v[X] = XT;
            Assert.AreEqual(v[src], srcT);
            Assert.AreEqual(v[dst], dstT);
            foreach (var t in dict)
            {
                v[equ] = t.Key;
                var tmp = string.Format(@"Source: {0}", t.Key);
                Assert.AreEqual(t.Value, v[equ], tmp);
            }
        }
    }
}
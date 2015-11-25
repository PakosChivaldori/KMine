using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace kmine_bump
{
    class Program
    {
        static void Main(string[] args)
        {
            var bs = File.ReadAllLines(@"bump.script");
            Script s = new Script();
            foreach (var q in bs)
                s.Parse(q);
            s.Run();
        }
    }
}

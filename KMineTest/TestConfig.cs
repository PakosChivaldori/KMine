using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using kmine;

namespace KMineTest
{
    class TestConfig : Config
    {
        public void ReadConfig(string[] c) { foreach (var s in c) ReadConfig(s); }
    }
}

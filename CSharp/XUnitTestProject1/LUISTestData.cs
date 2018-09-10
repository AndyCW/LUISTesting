using System;
using System.Collections.Generic;
using System.Text;

namespace XUnitTestProject1
{
    public class Entity
    {
        public string entity { get; set; }
        public int startPos { get; set; }
        public int endPos { get; set; }
    }

    public class LuisTestCaseData
    {
        public string text { get; set; }
        public string intent { get; set; }
        public List<Entity> entities { get; set; }
    }
}

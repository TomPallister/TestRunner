using System.Collections.Generic;

namespace TestRunner.Framework.Concrete.Model
{
    public class TestClass
    {
        public List<TestMethod> TestMethods { get; set; } 
        public string Name { get; set; }

        public TestClass()
        {
            TestMethods = new List<TestMethod>();
        }
    }
}

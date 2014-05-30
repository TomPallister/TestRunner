using System.Collections.Generic;

namespace TestRunner.Framework.Concrete.Model
{
    public class TestLibraryContainer
    {
        public List<TestClass> TestClasses { get; set; }

        public TestLibraryContainer()
        {
            TestClasses = new List<TestClass>();
        }
    }
}

using System;

namespace TestRunner.Framework.Concrete.Model
{
    public class TestRun 
    {
        public int Id { get; set; }
        public Guid TestRunIdentifier { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int TestCount { get; set; }
        public string TestRunStart { get; set; }
        public string TestRunEnd { get; set; }
        public string Exception { get; set; }
        public string Project { get; set; }
        public string EnvrionmentUrl { get; set; }
    }
}

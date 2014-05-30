using System;

namespace TestRunner.Framework.Concrete.Model
{
    public class TestCoverage
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string ModuleName { get; set; }
        public string TestName { get; set; }
        public Guid TestRunIdentifier { get; set; }
    }
}

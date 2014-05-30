using System;

namespace TestRunner.Framework.Concrete.Model
{
    public class TestRecord 
    {
        public int Id { get; set; }
        public int AssertCount { get; set; }
        public int TestRunKey { get; set; }

        public bool Executed { get; set; }
        public bool HasResults { get; set; }
        public bool IsError { get; set; }
        public bool IsFailure { get; set; }
        public bool IsSuccess { get; set; }

        public double Time { get; set; }
        public DateTime DateOfTest { get; set; }
        public Guid TestRunIdentifier { get; set; }

        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Project { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string ModuleCovered { get; set; }
    }
}

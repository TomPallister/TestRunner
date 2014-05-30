using System.Collections.Generic;

namespace TestRunner.Framework.Concrete.Model
{
    public class BeginTest
    {
        public string Dll { get; set; }
        public string ProjectName { get; set; }
        public string EnvironmentUrl { get; set; }
        public List<string> Namespaces { get; set; }
        public TypeOfTestRun TypeOfTestRun { get; set; }
        public string PeakPeriodInMinutes { get; set; }
        public string RampUpPeriodInMinutes { get; set; }
        public string WindDownPeriodInMinutes { get; set; }
        public string RampUpUsers { get; set; }
        public string WindDownUsers { get; set; }
        public string PeakUsers { get; set; }
    }
    public enum TypeOfTestRun
    {
        Performance,
        Automated
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Core;
using TestRunner.Framework.Concrete.Model;

namespace TestRunner.Framework.Abstract.Interface
{
    public interface IParallelTestRunner
    {
        TestRun Execute(List<TestToRun> listOfTestsToRun, string projectName, string environmentUrl);

        TestRun ExecutePerformanceTest(List<TestToRun> listOfTestsToRun, string projectName, string environmentUrl,
            Guid testRunId);
        string GetTestRunNameFromListOfTestsToRun(List<TestToRun> listOfTestsToRun);
        BeginTest GetSettingsFromArgs(string[] args);
        TestResult Run(string[] args);
        TestResult RunPerformanceTest(string[] args, Guid testRunId);
        void Start(TestToRun testToRun, Guid guid);
        void StartPerformanceTest(TestToRun testToRun, Guid guid);
        StringBuilder GetErrorsFromBeginTestIfAny(BeginTest beginTest);
    }
}

using System;
using System.Collections.Generic;
using NUnit.Core;
using TestRunner.Framework.Concrete.Model;

namespace TestRunner.Framework.Abstract.Interface
{
    public interface ITestRunnerResultManager
    {
        TestRun GetTestRun(Guid guid);
        TestRun CreateTestRun(Guid guid, string name, string projectName, string environmentUrl);
        void UpdateTestRun(TestRun testRun);
        void SaveTestRun(TestRun testRun);
        void SaveTestRecord(TestRecord testRecord, TestRun testRun);
        TestResult CreateTestResult();
        TestResult GetFailedTest(TestResult testResult);
        TestRecord MapTestResultToTestRecord(TestResult testResult, TestRecord testRecord, TestRun testRun, Guid guid);
        TestResult GetTest(TestResult testResult);
        List<TestResult> GetTests(TestResult testResult);
    }
}

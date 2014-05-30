using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using NUnit.Core;
using TestRunner.Framework.Abstract.Interface;
using TestRunner.Framework.Concrete.Manager;
using TestRunner.Framework.Concrete.Model;

namespace TestRunner.Framework.Concrete.Object
{
    public class TestResultManager : ITestRunnerResultManager
    {
        #region Private Fields

        private readonly string _apiUrl = ConfigurationManager.AppSettings["apiUrl"];

        #endregion Private Fields

        #region Public Methods

        public TestRun GetTestRun(Guid guid)
        {
            using (var httpClient = new HttpClient())
            {
                using (var httpClientManager = new HttpClientManager(httpClient))
                {
                    var url = string.Format("{0}api/testrunapi?guid={1}", _apiUrl, guid);
                    return JsonConvert.DeserializeObject<TestRun>(httpClientManager.Get(url));

                }
            }   
        }

        public TestRun CreateTestRun(Guid guid, string name, string projectName, string environmentUrl)
        {
            var testRun = new TestRun
            {
                TestRunIdentifier = guid,
                Name = name,
                ShortName = GetProjectNameStringFromString(name),
                TestRunStart = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                TestCount = 0,
                Project = projectName,
                EnvrionmentUrl = environmentUrl
            };

            return testRun;
        }

        public void UpdateTestRun(TestRun testRun)
        {
            using (var httpClient = new HttpClient())
            {
                using (var httpClientManager = new HttpClientManager(httpClient))
                {
                    try
                    {
                        httpClientManager.Put(testRun, string.Format("{0}api/testrunapi", _apiUrl));
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine("There was a problem updating the Test Run object. The error is {0}", exception);
                    }
                }
            }   
        }

        public void SaveTestRun(TestRun testRun)
        {
            using (var httpClient = new HttpClient())
            {
                using (var httpClientManager = new HttpClientManager(httpClient))
                {
                    try
                    {
                        httpClientManager.Post(testRun, string.Format("{0}api/testrunapi", _apiUrl));
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine("There was a problem saving the Test Run object. The error is {0}", exception);
                    }
                }
            }     
        }

        public void SaveTestRecord(TestRecord testRecord, TestRun testRun)
        {
            using (var httpClient = new HttpClient())
            {
                using (var httpClientManager = new HttpClientManager(httpClient))
                {
                    try
                    {
                        testRecord.TestRunKey = testRun.Id;
                        httpClientManager.Post(testRecord, string.Format("{0}api/testrecordapi", _apiUrl));
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine("There was a problem saving the Test Record object. The error is {0}", exception);
                    }
                }
            }   
        }

        public TestResult CreateTestResult()
        {

            return new TestResult(new TestInfo(new TestName()));
        }

        public TestResult GetFailedTest(TestResult testResult)
        {
            if (testResult.Executed)
            {
                if (testResult.HasResults)
                {
                    if (testResult.IsFailure || testResult.IsError)
                        if (testResult.FailureSite == FailureSite.SetUp ||
                            testResult.FailureSite == FailureSite.TearDown)
                        {
                            return testResult;
                        }

                    return (from TestResult tR in testResult.Results select GetFailedTest(tR)).FirstOrDefault();
                }

                if (testResult.IsFailure || testResult.IsError)
                {
                    return testResult;
                }
            }

            return null;
        }

        /// <summary>
        /// A TestResult has a Results property that contains more results. Basically child results 
        /// you need to loop though these until you find the actual tests that ran.
        /// </summary>
        /// <param name="testResult"></param>
        /// <returns></returns>
        public List<TestResult> GetTests(TestResult testResult)
        {
            var testResults = new List<TestResult>();

                if (testResult != null && testResult.Results != null && testResult.Results.Count > 1)
                {
                    testResults.AddRange(testResult.Results.Cast<TestResult>());
                }
                else
                {
                    if (testResult == null || testResult.Results == null) return testResults;

                    foreach (TestResult tResult in testResult.Results)
                    {
                        if (tResult.Results != null)
                        {
                            return GetTests(tResult);
                        }

                        testResults.Add(tResult);
                    }
                }

            return testResults;
        }

        /// <summary>
        /// A TestResult has a Results property that contains more results. Basically child results 
        /// you need to loop though these until you find the actual test that ran.
        /// </summary>
        /// <param name="testResult"></param>
        /// <returns></returns>
        public TestResult GetTest(TestResult testResult)
        {
            return testResult.Results == null ? testResult : 
                (from TestResult tR in testResult.Results select GetTest(tR)).FirstOrDefault();
        }

        /// <summary>
        /// We create a TestRecord object that is passed into the test method but we dont want to update this so we
        /// create a new TestRecord to save to the API. This is the main reason for this methods existence.
        /// </summary>
        /// <param name="testResult"></param>
        /// <param name="testRecord"></param>
        /// <param name="testRun"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public TestRecord MapTestResultToTestRecord(TestResult testResult, TestRecord testRecord, TestRun testRun, Guid guid)
        {

            testRecord.Description = testResult.Description;
            testRecord.Executed = testResult.Executed;
            testRecord.FullName = testResult.FullName;
            testRecord.HasResults = testResult.HasResults;
            testRecord.IsError = testResult.IsError;
            testRecord.IsFailure = testResult.IsFailure;
            testRecord.IsSuccess = testResult.IsSuccess;
            testRecord.Message = testResult.Message;
            testRecord.Name = testResult.Name;
            testRecord.StackTrace = testResult.StackTrace;
            testRecord.Time = testResult.Time;
            testRecord.Project = testRun.Project;
            testRecord.ModuleCovered = testResult.Description;
            testRecord.TestRunKey = testRun.Id;
            testRecord.TestRunIdentifier = guid;

            if (!testResult.IsSuccess)
            {
                //the test failed map it as such
                testRecord.AssertCount = testResult.AssertCount;
                
            }

            return testRecord;
        }

        public string GetProjectNameStringFromString(string name)
        {
            var projectNamefirstIndex = name.LastIndexOf(@"\", StringComparison.Ordinal);

            if (projectNamefirstIndex == -1) return name;

            var projectName = name.Substring(projectNamefirstIndex + 1);
            var dllIndex = projectName.LastIndexOf(".dll", StringComparison.Ordinal);
            if (dllIndex > 0)
            {
                projectName = projectName.Substring(0, dllIndex);
            }
            return projectName;
        }

        #endregion Public Methods
    }
}

using System;
using System.Collections.Generic;
using TestRunner.Framework.Abstract.Interface;
using TestRunner.Framework.Concrete.Enum;
using TestRunner.Framework.Concrete.Manager;
using TestRunner.Framework.Concrete.Model;
using TestRunner.Framework.Concrete.Object;

namespace TestRunner
{
    class Program
    {

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Log4NetLogger.LogEntry(typeof(Program), "Application_Error", "Caught exception in Global Asax", LoggerLevel.Fatal, (Exception)e.ExceptionObject);
            Console.WriteLine(e.ToString());
        }

        static void Main(string[] args)
        {
            Log4NetLogger.LogEntry(typeof(Program), "Application_Start", "ApplicationStarted", LoggerLevel.Info);
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            var container = IoC.Bind();
            var assemblyScanner = container.GetInstance<IAssemblyScanner>();
            var parallelTestRunner = container.GetInstance<IParallelTestRunner>();

            var beginTest = parallelTestRunner.GetSettingsFromArgs(args);
            if (beginTest.TypeOfTestRun != TypeOfTestRun.Performance)
            {
                var errors = parallelTestRunner.GetErrorsFromBeginTestIfAny(beginTest);

                if (errors.Length > 0)
                {
                    Console.WriteLine(errors);
                    throw new Exception("The required paramaters to run a test were not provided.");
                }

                var testLibraryContainer = assemblyScanner.GetTestLibraryContainer(beginTest.Dll);
                var listOfTestToRuns = assemblyScanner.GetTestsToRun(testLibraryContainer, beginTest.Dll,
                    beginTest.ProjectName, beginTest.Namespaces);
                var i = 0;
                foreach (var listOfTestToRun in listOfTestToRuns)
                {
                    Console.WriteLine(i);
                    Console.WriteLine(listOfTestToRun.Arguments[0]);
                    i++;
                }

                if (beginTest.Namespaces != null)
                {
                    foreach (var name in beginTest.Namespaces)
                    {
                        Console.WriteLine("namespace= {0}", name);
                    }
                }

                var testRun = parallelTestRunner.Execute(listOfTestToRuns, beginTest.ProjectName,
                    beginTest.EnvironmentUrl);
                Console.WriteLine("The Test Run Id is {0}", testRun.Id);
                Console.WriteLine("The Test Run Start date is {0}", testRun.TestRunStart);
                Console.WriteLine("The Test Run End date is {0}", testRun.TestRunEnd);
                Console.WriteLine("{0} Ran", testRun.TestCount);
                Console.WriteLine(
                    "The Url to view the test results is http://testdata-app.fourth.cloud/api/testrunapi/{0}",
                    testRun.Id);
                Log4NetLogger.LogEntry(typeof (Program), "Application_End", "ApplicationEnded", LoggerLevel.Info);
            }
            else
            {
                var errors = parallelTestRunner.GetErrorsFromBeginTestIfAny(beginTest);
                if (errors.Length > 0)
                {
                    Console.WriteLine(errors);
                    throw new Exception("The required paramaters to run a test were not provided.");
                }
                var testLibraryContainer = assemblyScanner.GetTestLibraryContainer(beginTest.Dll);
                var listOfTestToRuns = assemblyScanner.GetTestsToRun(testLibraryContainer, beginTest.Dll,
                    beginTest.ProjectName, beginTest.Namespaces);
                var i = 0;
                foreach (var listOfTestToRun in listOfTestToRuns)
                {
                    Console.WriteLine(i);
                    Console.WriteLine(listOfTestToRun.Arguments[0]);
                    i++;
                }
                if (beginTest.Namespaces != null)
                {
                    foreach (var name in beginTest.Namespaces)
                    {
                        Console.WriteLine("namespace= {0}", name);
                    }
                }
                var testRunIdentifier = Guid.NewGuid();
                var projectName = parallelTestRunner.GetTestRunNameFromListOfTestsToRun(listOfTestToRuns);
                var testRun = new TestRun()
                {
                    TestRunIdentifier = testRunIdentifier,
                    Project = beginTest.ProjectName,
                    EnvrionmentUrl = beginTest.EnvironmentUrl,
                    ShortName = GetProjectNameStringFromString(projectName),
                    Name = projectName,
                    TestRunStart = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    TestCount = listOfTestToRuns.Count
                };

                var testRunnerResultManager = container.GetInstance<ITestRunnerResultManager>();
                testRunnerResultManager.SaveTestRun(testRun);

                if (!string.IsNullOrWhiteSpace(beginTest.RampUpPeriodInMinutes))
                {
                    RunSectionOfTest(beginTest.RampUpPeriodInMinutes, beginTest.RampUpUsers, beginTest.EnvironmentUrl, beginTest.ProjectName, listOfTestToRuns, parallelTestRunner, testRun);
                }

                //if (!string.IsNullOrWhiteSpace(beginTest.PeakPeriodInMinutes))
                //{
                //    RunSectionOfTest(beginTest.PeakPeriodInMinutes, beginTest.PeakUsers, beginTest.EnvironmentUrl, beginTest.ProjectName, listOfTestToRuns, parallelTestRunner, testRun);
                //}

                //if (!string.IsNullOrWhiteSpace(beginTest.WindDownPeriodInMinutes))
                //{
                //    RunSectionOfTest(beginTest.WindDownPeriodInMinutes, beginTest.WindDownUsers, beginTest.EnvironmentUrl, beginTest.ProjectName, listOfTestToRuns, parallelTestRunner, testRun);
                //}

                //testRun = testRunnerResultManager.GetTestRun(testRunIdentifier);
                //testRun.TestRunEnd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                //testRunnerResultManager.UpdateTestRun(testRun);


                Console.WriteLine("The Test Run Id is {0}", testRun.Id);
                Console.WriteLine("The Test Run Start date is {0}", testRun.TestRunStart);
                Console.WriteLine("The Test Run End date is {0}", testRun.TestRunEnd);
                Console.WriteLine("{0} Ran", testRun.TestCount);
                Console.WriteLine(
                    "The Url to view the test results is http://testdata-app.fourth.cloud/api/testrunapi/{0}",
                    testRun.Id);
                Log4NetLogger.LogEntry(typeof(Program), "Application_End", "ApplicationEnded", LoggerLevel.Info);


            }
            
        }

        private static void RunSectionOfTest(string periodInMinutes, string users, string environmentUrl, string projectName, List<TestToRun> listOfTestToRuns, IParallelTestRunner parallelTestRunner, TestRun testRun)
        {
            if (!string.IsNullOrWhiteSpace(periodInMinutes) && !string.IsNullOrWhiteSpace(users))
            {
                var endTime = DateTime.Now.AddMinutes(Convert.ToInt32(periodInMinutes));
                while (DateTime.Now < endTime)
                {
                    for (int j = 0; j < Convert.ToInt32(users); j++)
                    {
                        testRun = parallelTestRunner.ExecutePerformanceTest(listOfTestToRuns, projectName,
                            environmentUrl, testRun.TestRunIdentifier);
                    }
                }
            }
        }

        public static string GetProjectNameStringFromString(string name)
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
    }
}

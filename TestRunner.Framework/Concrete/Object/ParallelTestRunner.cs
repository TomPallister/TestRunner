using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.ConsoleRunner;
using NUnit.Core;
using NUnit.Util;
using TestRunner.Framework.Abstract.Interface;
using TestRunner.Framework.Concrete.Model;

namespace TestRunner.Framework.Concrete.Object
{
    public class ParallelTestRunner : IParallelTestRunner
    {
        #region Static Fields

        private static readonly Logger Log = InternalTrace.GetLogger(typeof (Runner));

        #endregion Static Fields

        #region Private Fields

        private readonly ITestRunnerResultManager _testRunnerResultManager;
        private int _testCount;
        private TestRun _testRun;
        private Guid _thisTestRunIdentifier;

        #endregion Private Fields

        #region Constructor

        public ParallelTestRunner(ITestRunnerResultManager testRunnerResultManager)
        {
            _testRunnerResultManager = testRunnerResultManager;
        }

        #endregion Constructor

        #region Public Methods

        public TestRun Execute(List<TestToRun> listOfTestsToRun, string projectName, string environmentUrl)
        {
            //set up test run guid
            _thisTestRunIdentifier = Guid.NewGuid();

            //create and save the test run
            _testRun = _testRunnerResultManager.CreateTestRun(_thisTestRunIdentifier,
                GetTestRunNameFromListOfTestsToRun(listOfTestsToRun), projectName, environmentUrl);
            _testRun.TestCount = listOfTestsToRun.Count;
            _testRunnerResultManager.SaveTestRun(_testRun);

            //loop through the tests and run them save the results 
            Parallel.ForEach(listOfTestsToRun,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Convert.ToInt32(ConfigurationManager.AppSettings["MaxDegreeOfParallelism"])
                }, testToRun =>
                {
                    _testCount++;
                    Start(testToRun, _thisTestRunIdentifier);
                });

            //save the final test run status
            _testRun = _testRunnerResultManager.GetTestRun(_thisTestRunIdentifier);
            _testRun.TestRunEnd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _testRun.TestCount = _testRunnerResultManager.GetTestRun(_thisTestRunIdentifier).TestCount;
            _testRunnerResultManager.UpdateTestRun(_testRun);
            return _testRun;
        }

        public TestRun ExecutePerformanceTest(List<TestToRun> listOfTestsToRun, string projectName,
            string environmentUrl, Guid testRunId)
        {
            _thisTestRunIdentifier = testRunId;
            //loop through the tests and run them save the results 
            Parallel.ForEach(listOfTestsToRun,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Convert.ToInt32(ConfigurationManager.AppSettings["MaxDegreeOfParallelism"])
                }, testToRun =>
                {
                    _testCount++;
                    StartPerformanceTest(testToRun, _thisTestRunIdentifier);
                });

            //save the final test run status
            _testRun = _testRunnerResultManager.GetTestRun(_thisTestRunIdentifier);
            _testRun.TestRunEnd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _testRun.TestCount = _testRunnerResultManager.GetTestRun(_thisTestRunIdentifier).TestCount;
            _testRunnerResultManager.UpdateTestRun(_testRun);
            return _testRun;
        }

        public string GetTestRunNameFromListOfTestsToRun(List<TestToRun> listOfTestsToRun)
        {
            TestToRun firstOrDefault = listOfTestsToRun.FirstOrDefault();
            if (firstOrDefault != null)
                return firstOrDefault.Arguments[1];

            return "Could not find name.";
        }

        public BeginTest GetSettingsFromArgs(string[] args)
        {
            var beginTest = new BeginTest
            {
                TypeOfTestRun = TypeOfTestRun.Automated
            };

            foreach (string argument in args.ToList())
            {
                if (argument.Contains("-dll:"))
                {
                    beginTest.Dll = GetSetting(argument);
                }
                else if (argument.Contains("-project:"))
                {
                    beginTest.ProjectName = GetSetting(argument);
                }
                else if (argument.Contains("-url:"))
                {
                    beginTest.EnvironmentUrl = GetSetting(argument);
                }
                else if (argument.Contains("-namespace:"))
                {
                    beginTest.Namespaces = GetNamespacesToRun(argument);
                }
                else if (argument.Contains("-type:performance"))
                {
                    beginTest.TypeOfTestRun = TypeOfTestRun.Performance;
                }
                else if (argument.Contains("-rampuptime:"))
                {
                    beginTest.RampUpPeriodInMinutes = GetSetting(argument);
                }
                else if (argument.Contains("-rampupusers:"))
                {
                    beginTest.RampUpUsers = GetSetting(argument);
                }
                else if (argument.Contains("-winddowntime:"))
                {
                    beginTest.WindDownPeriodInMinutes = GetSetting(argument);
                }
                else if (argument.Contains("-winddownusers:"))
                {
                    beginTest.WindDownUsers = GetSetting(argument);
                }
                else if (argument.Contains("-timeoftestrun:"))
                {
                    beginTest.WindDownUsers = GetSetting(argument);
                }
                else if (argument.Contains("-peakusers:"))
                {
                    beginTest.WindDownUsers = GetSetting(argument);
                }
            }

            return beginTest;
        }

        public TestResult Run(string[] args)
        {
            string[] opt = args;

            var options = new ConsoleOptions(opt);

            // Create SettingsService early so we know the trace level right at the start
            var settingsService = new SettingsService();
            var level =
                (InternalTraceLevel)
                    settingsService.GetSetting("Options.InternalTraceLevel", InternalTraceLevel.Default);
            if (options.trace != InternalTraceLevel.Default)
                level = options.trace;

            InternalTrace.Initialize("nunit-console_%p.log", level);

            Log.Info("NUnit-console.exe starting");

            if (options.help)
            {
                options.Help();
                return null;
                //return NUnitConsoleUi.OK;
            }

            if (options.cleanup)
            {
                Log.Info("Performing cleanup of shadow copy cache");
                DomainManager.DeleteShadowCopyPath();
                Console.WriteLine("Shadow copy cache emptied");
                return null;
                //return NUnitConsoleUi.OK;
            }

            if (options.NoArgs)
            {
                Console.Error.WriteLine("fatal error: no inputs specified");
                options.Help();
                return null;
                //return NUnitConsoleUi.OK;
            }

            if (!options.Validate())
            {
                foreach (string arg in options.InvalidArguments)
                    Console.Error.WriteLine("fatal error: invalid argument: {0}", arg);
                options.Help();
                return null;
                //return NUnitConsoleUi.InvalidArg;
            }

            // Add Standard Services to ServiceManager
            ServiceManager.Services.AddService(settingsService);
            ServiceManager.Services.AddService(new DomainManager());
            //ServiceManager.Services.AddService( new RecentFilesService() );
            ServiceManager.Services.AddService(new ProjectService());
            //ServiceManager.Services.AddService( new TestLoader() );
            ServiceManager.Services.AddService(new AddinRegistry());
            ServiceManager.Services.AddService(new AddinManager());
            ServiceManager.Services.AddService(new TestAgency());

            // Initialize Services
            ServiceManager.Services.InitializeServices();

            foreach (string parm in options.Parameters)
            {
                if (!Services.ProjectService.CanLoadProject(parm) && !PathUtils.IsAssemblyFileType(parm))
                {
                    Console.WriteLine("File type not known: {0}", parm);
                    return null;
                    //return NUnitConsoleUi.InvalidArg;
                }
            }

            try
            {
                var consoleUi = new NUnitConsoleUi();
                return consoleUi.Execute(options);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
                //return NUnitConsoleUi.FileNotFound;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled Exception:\n{0}", ex);
                return null;
                //return NUnitConsoleUi.UnexpectedError;
            }
            finally
            {
                if (options.wait)
                {
                    Console.Out.WriteLine("\nHit <enter> key to continue");
                    Console.ReadLine();
                }

                Log.Info("NUnit-console.exe terminating");
            }
        }

        public TestResult RunPerformanceTest(string[] args, Guid testRunId)
        {
            AppDomain.CurrentDomain.SetData("TestRunId", testRunId);
            string[] opt = args;

            var options = new ConsoleOptions(opt);

            // Create SettingsService early so we know the trace level right at the start
            var settingsService = new SettingsService();
            var level =
                (InternalTraceLevel)
                    settingsService.GetSetting("Options.InternalTraceLevel", InternalTraceLevel.Default);
            if (options.trace != InternalTraceLevel.Default)
                level = options.trace;

            InternalTrace.Initialize("nunit-console_%p.log", level);

            Log.Info("NUnit-console.exe starting");

            if (options.help)
            {
                options.Help();
                return null;
                //return NUnitConsoleUi.OK;
            }

            if (options.cleanup)
            {
                Log.Info("Performing cleanup of shadow copy cache");
                DomainManager.DeleteShadowCopyPath();
                Console.WriteLine("Shadow copy cache emptied");
                return null;
                //return NUnitConsoleUi.OK;
            }

            if (options.NoArgs)
            {
                Console.Error.WriteLine("fatal error: no inputs specified");
                options.Help();
                return null;
                //return NUnitConsoleUi.OK;
            }

            if (!options.Validate())
            {
                foreach (string arg in options.InvalidArguments)
                    Console.Error.WriteLine("fatal error: invalid argument: {0}", arg);
                options.Help();
                return null;
                //return NUnitConsoleUi.InvalidArg;
            }

            // Add Standard Services to ServiceManager
            ServiceManager.Services.AddService(settingsService);
            ServiceManager.Services.AddService(new DomainManager());
            //ServiceManager.Services.AddService( new RecentFilesService() );
            ServiceManager.Services.AddService(new ProjectService());
            //ServiceManager.Services.AddService( new TestLoader() );
            ServiceManager.Services.AddService(new AddinRegistry());
            ServiceManager.Services.AddService(new AddinManager());
            ServiceManager.Services.AddService(new TestAgency());

            // Initialize Services
            ServiceManager.Services.InitializeServices();

            foreach (string parm in options.Parameters)
            {
                if (!Services.ProjectService.CanLoadProject(parm) && !PathUtils.IsAssemblyFileType(parm))
                {
                    Console.WriteLine("File type not known: {0}", parm);
                    return null;
                    //return NUnitConsoleUi.InvalidArg;
                }
            }

            try
            {
                var consoleUi = new NUnitConsoleUi();
                TestResult result = consoleUi.Execute(options);
                result.StackTrace = testRunId.ToString();
                return result;
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
                //return NUnitConsoleUi.FileNotFound;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unhandled Exception:\n{0}", ex);
                return null;
                //return NUnitConsoleUi.UnexpectedError;
            }
            finally
            {
                if (options.wait)
                {
                    Console.Out.WriteLine("\nHit <enter> key to continue");
                    Console.ReadLine();
                }

                Log.Info("NUnit-console.exe terminating");
            }
        }


        public void Start(TestToRun testToRun, Guid guid)
        {
            //we clone our arguments  but might not need to, need to investigate
            string[] test = CloneObject(testToRun.Arguments);

            //we lock the arguments so that we dont get any bad threading stuff
            lock (test)
            {
                Parallel.Invoke(() =>
                {
                    //set up and run our task
                    TestResult testResult = _testRunnerResultManager.CreateTestResult();

                    Task<TestResult> task = Task.Factory.StartNew(() =>
                        testResult = Run(test)
                        );

                    //some crap i got of the net
                    Task<TaskStatus> continuation = task.ContinueWith(antecedent => antecedent.Status);

                    if (continuation.Result == TaskStatus.RanToCompletion)
                    {
                        //the task ran to completion! Woo.
                        //load up our test run. This probably needs sorting.
                        TestRun testRun = _testRunnerResultManager.GetTestRun(guid);


                        List<TestResult> testResults = _testRunnerResultManager.GetTests(testResult);

                        foreach (TestResult result in testResults)
                        {
                            //map our initial record and result so we have the right information to save.
                            TestRecord recordToSave = _testRunnerResultManager.MapTestResultToTestRecord(result,
                                testToRun.TestRecord, testRun, guid);

                            //save our record.
                            _testRunnerResultManager.SaveTestRecord(recordToSave, testRun);
                        }
                    }
                    else
                    {
                        //the task failed!!! Ohh dear.
                        //get the test run so we have its ID
                        TestRun testRun = _testRunnerResultManager.GetTestRun(guid);

                        //get the real test from the test result object NUnit has kindly passed to us.
                        TestResult resultOfTest = _testRunnerResultManager.GetTest(testResult);

                        //map our initial record and result so we have the right information to save.
                        TestRecord recordToSave = _testRunnerResultManager.MapTestResultToTestRecord(resultOfTest,
                            testToRun.TestRecord, testRun, guid);

                        //some custom stuff becasue of the error
                        if (continuation.Exception != null)
                        {
                            testToRun.TestRecord.StackTrace = continuation.Exception.ToString();
                        }
                        if (continuation.Status != TaskStatus.RanToCompletion)
                        {
                            testToRun.TestRecord.Message = continuation.Status.ToString();
                        }

                        //save our record.
                        _testRunnerResultManager.SaveTestRecord(recordToSave, testRun);
                    }
                });
            }
        }

        public void StartPerformanceTest(TestToRun testToRun, Guid guid)
        {
            //we clone our arguments  but might not need to, need to investigate
            string[] test = CloneObject(testToRun.Arguments);

            //we lock the arguments so that we dont get any bad threading stuff
            lock (test)
            {
                Parallel.Invoke(() =>
                {
                    //set up and run our task
                    TestResult testResult = _testRunnerResultManager.CreateTestResult();

                    Task<TestResult> task = Task.Factory.StartNew(() =>
                        testResult = RunPerformanceTest(test, guid)
                        );

                    //some crap i got of the net
                    Task<TaskStatus> continuation = task.ContinueWith(antecedent => antecedent.Status);

                    if (continuation.Result == TaskStatus.RanToCompletion)
                    {
                        //the task ran to completion! Woo.
                        //load up our test run. This probably needs sorting.
                        TestRun testRun = _testRunnerResultManager.GetTestRun(guid);
                        List<TestResult> testResults = _testRunnerResultManager.GetTests(testResult);

                        foreach (TestResult result in testResults)
                        {
                            //map our initial record and result so we have the right information to save.
                            TestRecord recordToSave = _testRunnerResultManager.MapTestResultToTestRecord(result,
                                testToRun.TestRecord, testRun, guid);

                            //save our record.
                            _testRunnerResultManager.SaveTestRecord(recordToSave, testRun);
                        }
                    }
                    else
                    {
                        //the task failed!!! Ohh dear.
                        //get the test run so we have its ID
                        TestRun testRun = _testRunnerResultManager.GetTestRun(guid);

                        //get the real test from the test result object NUnit has kindly passed to us.
                        TestResult resultOfTest = _testRunnerResultManager.GetTest(testResult);

                        //map our initial record and result so we have the right information to save.
                        TestRecord recordToSave = _testRunnerResultManager.MapTestResultToTestRecord(resultOfTest,
                            testToRun.TestRecord, testRun, guid);

                        //some custom stuff becasue of the error
                        if (continuation.Exception != null)
                        {
                            testToRun.TestRecord.StackTrace = continuation.Exception.ToString();
                        }
                        if (continuation.Status != TaskStatus.RanToCompletion)
                        {
                            testToRun.TestRecord.Message = continuation.Status.ToString();
                        }

                        //save our record.
                        _testRunnerResultManager.SaveTestRecord(recordToSave, testRun);
                    }
                });
            }
        }


        public StringBuilder GetErrorsFromBeginTestIfAny(BeginTest beginTest)
        {
            var errorList = new StringBuilder();

            if (beginTest.Dll == null)
            {
                errorList.Append("A dll was not provided.");
                errorList.Append("Please pass in -dll:[dlllocation] without the square brackets");
            }
            if (beginTest.EnvironmentUrl == null)
            {
                errorList.Append("A EnvironmentUrl was not provided.");
                errorList.Append("Please pass in -url:[url] without the square brackets");
            }
            if (beginTest.ProjectName == null)
            {
                errorList.Append("A ProjectName was not provided.");
                errorList.Append("Please pass in -project:[projectname] without the square brackets");
            }

            return errorList;
        }

        public string[] CloneObject(string[] arg)
        {
            object obj = arg.Clone();

            string[] arr = ((IEnumerable) obj).Cast<object>()
                .Select(x => x.ToString())
                .ToArray();

            return arr;
        }

        public string GetNameFromArguments(string[] arguments)
        {
            return arguments[1];
        }

        #endregion Public Methods

        #region Private Methods

        public string GetSetting(string arg)
        {
            int startIndex = arg.IndexOf(@":", StringComparison.Ordinal);
            return arg.Substring(startIndex + 1);
        }

        public List<string> GetNamespacesToRun(string arg)
        {
            int startIndex = arg.IndexOf(@":", StringComparison.Ordinal);
            string stringToWorkWith = arg.Substring(startIndex + 1);
            string trimString = stringToWorkWith.Replace(" ", string.Empty);
            string[] returnString = trimString.Trim().Split(',');

            return returnString.ToList();
        }

        #endregion Private Methods
    }
}
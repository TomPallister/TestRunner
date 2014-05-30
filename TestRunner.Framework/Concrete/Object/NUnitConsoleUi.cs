using System;
using System.IO;
using NUnit.ConsoleRunner;
using NUnit.Core;
using NUnit.Core.Filters;
using NUnit.Util;

namespace TestRunner.Framework.Concrete.Object
{
    public class NUnitConsoleUi
    {
        #region Public Fields

        public static readonly int OK = 0;
        public static readonly int InvalidArg = -1;
        public static readonly int FileNotFound = -2;
        public static readonly int FixtureNotFound = -3;
        public static readonly int UnexpectedError = -100;

        #endregion Public Fields

        #region Private Fields

        private string _workDir;

        #endregion Private Fields

        #region Public Methods

        public TestResult Execute(ConsoleOptions options)
        {

            _workDir = options.work;
            if (string.IsNullOrEmpty(_workDir))
                _workDir = Environment.CurrentDirectory;
            else
            {
                _workDir = Path.GetFullPath(_workDir);
                if (!Directory.Exists(_workDir))
                    Directory.CreateDirectory(_workDir);
            }

            TextWriter outWriter = Console.Out;
            bool redirectOutput = !string.IsNullOrEmpty(options.output);
            if (redirectOutput)
            {
                StreamWriter outStreamWriter = new StreamWriter(Path.Combine(_workDir, options.output));
                outStreamWriter.AutoFlush = true;
                outWriter = outStreamWriter;
            }

            TextWriter errorWriter = Console.Error;
            bool redirectError = !string.IsNullOrEmpty(options.err);
            if (redirectError)
            {
                StreamWriter errorStreamWriter = new StreamWriter(Path.Combine(_workDir, options.err));
                errorStreamWriter.AutoFlush = true;
                errorWriter = errorStreamWriter;
            }

            TestPackage package = MakeTestPackage(options);

            ProcessModel processModel = package.Settings.Contains("ProcessModel")
                ? (ProcessModel)package.Settings["ProcessModel"]
                : ProcessModel.Default;

            DomainUsage domainUsage = package.Settings.Contains("DomainUsage")
                ? (DomainUsage)package.Settings["DomainUsage"]
                : DomainUsage.Default;

            RuntimeFramework framework = package.Settings.Contains("RuntimeFramework")
                ? (RuntimeFramework)package.Settings["RuntimeFramework"]
                : RuntimeFramework.CurrentFramework;

#if CLR_2_0 || CLR_4_0
            Console.WriteLine("ProcessModel: {0}    DomainUsage: {1}", processModel, domainUsage);

            Console.WriteLine("Execution Runtime: {0}", framework);
#else
            Console.WriteLine("DomainUsage: {0}", domainUsage);

            if (processModel != ProcessModel.Default && processModel != ProcessModel.Single)
                Console.WriteLine("Warning: Ignoring project setting 'processModel={0}'", processModel);

            if (!RuntimeFramework.CurrentFramework.Supports(framework))
                Console.WriteLine("Warning: Ignoring project setting 'runtimeFramework={0}'", framework);
#endif

            using (NUnit.Core.TestRunner testRunner = new DefaultTestRunnerFactory().MakeTestRunner(package))
            {
                testRunner.Load(package);

                if (testRunner.Test == null)
                {
                    testRunner.Unload();
                    Console.Error.WriteLine("Unable to locate fixture {0}", options.fixture);
                    return null;
                }

                EventCollector collector = new EventCollector(options, outWriter, errorWriter);

                TestFilter testFilter;

                if (!CreateTestFilter(options, out testFilter))
                    return null;

                string savedDirectory = Environment.CurrentDirectory;
                TextWriter savedOut = Console.Out;
                TextWriter savedError = Console.Error;

                try
                {
                     return testRunner.Run(collector, testFilter, false, LoggingThreshold.Off);
                }
                finally
                {
                    outWriter.Flush();
                    errorWriter.Flush();

                    if (redirectOutput)
                        outWriter.Close();

                    if (redirectError)
                        errorWriter.Close();

                    Environment.CurrentDirectory = savedDirectory;
                    Console.SetOut(savedOut);
                    Console.SetError(savedError);
                }
            }
        }

        #endregion Public Methods

        #region Internal Methods

        internal static bool CreateTestFilter(ConsoleOptions options, out TestFilter testFilter)
        {
            testFilter = TestFilter.Empty;

            SimpleNameFilter nameFilter = new SimpleNameFilter();

            if (options.run != null && options.run != string.Empty)
            {
                Console.WriteLine("Selected test(s): " + options.run);

                foreach (string name in TestNameParser.Parse(options.run))
                    nameFilter.Add(name);

                testFilter = nameFilter;
            }

            if (options.runlist != null && options.runlist != string.Empty)
            {
                Console.WriteLine("Run list: " + options.runlist);

                try
                {
                    using (StreamReader rdr = new StreamReader(options.runlist))
                    {
                        // NOTE: We can't use rdr.EndOfStream because it's
                        // not present in .NET 1.x.
                        string line = rdr.ReadLine();
                        while (line != null && line.Length > 0)
                        {
                            if (line[0] != '#')
                                nameFilter.Add(line);
                            line = rdr.ReadLine();
                        }
                    }
                }
                catch (Exception e)
                {
                    if (e is FileNotFoundException || e is DirectoryNotFoundException)
                    {
                        Console.WriteLine("Unable to locate file: " + options.runlist);
                        return false;
                    }
                    throw;
                }

                testFilter = nameFilter;
            }

            if (!string.IsNullOrEmpty(options.include))
            {
                TestFilter includeFilter = new CategoryExpression(options.include).Filter;
                Console.WriteLine("Included categories: " + includeFilter);

                if (testFilter.IsEmpty)
                    testFilter = includeFilter;
                else
                    testFilter = new AndFilter(testFilter, includeFilter);
            }

            if (!string.IsNullOrEmpty(options.exclude))
            {
                TestFilter excludeFilter = new NotFilter(new CategoryExpression(options.exclude).Filter);
                Console.WriteLine("Excluded categories: " + excludeFilter);

                if (testFilter.IsEmpty)
                    testFilter = excludeFilter;
                else if (testFilter is AndFilter)
                    ((AndFilter)testFilter).Add(excludeFilter);
                else
                    testFilter = new AndFilter(testFilter, excludeFilter);
            }

            if (testFilter is NotFilter)
                ((NotFilter)testFilter).TopLevel = true;

            return true;
        }

        #endregion Internal Methods

        #region Private Methods

        private TestPackage MakeTestPackage(ConsoleOptions options)
        {
            TestPackage package;
            DomainUsage domainUsage;
            ProcessModel processModel = ProcessModel.Default;
            RuntimeFramework framework = null;

            string[] parameters = new string[options.ParameterCount];
            for (int i = 0; i < options.ParameterCount; i++)
                parameters[i] = Path.GetFullPath((string)options.Parameters[i]);

            if (options.IsTestProject)
            {
                NUnitProject project =
                    Services.ProjectService.LoadProject(parameters[0]);

                string configName = options.config;
                if (configName != null)
                    project.SetActiveConfig(configName);

                package = project.ActiveConfig.MakeTestPackage();
                processModel = project.ProcessModel;
                domainUsage = project.DomainUsage;
                framework = project.ActiveConfig.RuntimeFramework;
            }
            else if (parameters.Length == 1)
            {
                package = new TestPackage(parameters[0]);
                domainUsage = DomainUsage.Single;
            }
            else
            {
                // TODO: Figure out a better way to handle "anonymous" packages
                package = new TestPackage(null, parameters);
                package.AutoBinPath = true;
                domainUsage = DomainUsage.Multiple;
            }

            if (options.basepath != null && options.basepath != string.Empty)
            {
                package.BasePath = options.basepath;
            }

            if (options.privatebinpath != null && options.privatebinpath != string.Empty)
            {
                package.AutoBinPath = false;
                package.PrivateBinPath = options.privatebinpath;
            }

#if CLR_2_0 || CLR_4_0
            if (options.framework != null)
                framework = RuntimeFramework.Parse(options.framework);

            if (options.process != ProcessModel.Default)
                processModel = options.process;
#endif

            if (options.domain != DomainUsage.Default)
                domainUsage = options.domain;

            package.TestName = options.fixture;

            package.Settings["ProcessModel"] = processModel;
            package.Settings["DomainUsage"] = domainUsage;

            if (framework != null)
                package.Settings["RuntimeFramework"] = framework;

            if (domainUsage == DomainUsage.None)
            {
                // Make sure that addins are available
                CoreExtensions.Host.AddinRegistry = Services.AddinRegistry;
            }

            package.Settings["ShadowCopyFiles"] = !options.noshadow;
            package.Settings["UseThreadedRunner"] = !options.nothread;
            package.Settings["DefaultTimeout"] = options.timeout;
            package.Settings["WorkDirectory"] = _workDir;
            package.Settings["StopOnError"] = options.stoponerror;

            if (options.apartment != System.Threading.ApartmentState.Unknown)
                package.Settings["ApartmentState"] = options.apartment;

            return package;
        }

        #endregion Private Methods
    }
}


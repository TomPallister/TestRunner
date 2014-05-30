using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using TestRunner.Framework.Abstract.Interface;
using TestRunner.Framework.Concrete.Model;
using TestRunner.Framework.Concrete.Object;

namespace TestRunner.UnitTests
{
    [TestFixture]
    public class CommandLineParameterTests
    {
        private string[] _argsThree;
        private string[] _argsFour;
        private string _argDll;
        private string _argProjectName;
        private string _argEnvrionmentUrl;
        private string _argNamespaces;

        private string _resultDll;
        private string _resultProjectName;
        private string _resultEnvrionmentUrl;

        private List<string> _resultNamespaces = new List<string>
        {
            "Fourth.R9.SmokeTests.HR.Employees.CreateEmployee",
            "Fourth.R9.SmokeTests.HR.Employees.EMPAudit"
        };

        private IParallelTestRunner _parallelTestRunner;

        [SetUp]
        public void SetUp()
        {
            _argDll = @"-dll:C:\a\path\to\a\dll.dll";
            _argProjectName = @"-project:aProjectName";
            _argEnvrionmentUrl = @"-url:aEnvironmentUrl";
            _argNamespaces =
                @"-namespace:Fourth.R9.SmokeTests.HR.Employees.CreateEmployee, Fourth.R9.SmokeTests.HR.Employees.EMPAudit";

            _resultDll = @"C:\a\path\to\a\dll.dll";
            _resultProjectName = @"aProjectName";
            _resultEnvrionmentUrl = @"aEnvironmentUrl";

            _argsThree = new[]
            {
                _argDll, 
                _argProjectName, 
                _argEnvrionmentUrl,
            };

            _argsFour = new[]
            {
                _argDll, 
                _argProjectName, 
                _argEnvrionmentUrl,
                _argNamespaces
            };

            _parallelTestRunner = new ParallelTestRunner(new TestResultManager());
        }

        [Test]
        public void TakeParametersAndReturnTheThreeStringsINeed()
        {
            var beginTest = _parallelTestRunner.GetSettingsFromArgs(_argsThree);

            beginTest.Dll.Should().Be(_resultDll);
            beginTest.ProjectName.Should().Be(_resultProjectName);
            beginTest.EnvironmentUrl.Should().Be(_resultEnvrionmentUrl);
            beginTest.Namespaces.Should().BeNull();
        }

        [Test]
        public void TakeParametersAndReturnTheFourStringsINeed()
        {
            var beginTest = _parallelTestRunner.GetSettingsFromArgs(_argsFour);

            beginTest.Dll.Should().Be(_resultDll);
            beginTest.ProjectName.Should().Be(_resultProjectName);
            beginTest.EnvironmentUrl.Should().Be(_resultEnvrionmentUrl);
            beginTest.Namespaces.Should().BeEquivalentTo(_resultNamespaces);
        }

        [Test]
        public void CheckTheBeginTest()
        {
            var beginTest = new BeginTest
            {
                Dll = _resultDll,
                EnvironmentUrl = _resultEnvrionmentUrl,
                ProjectName = _resultProjectName,
                Namespaces = _resultNamespaces
            };

            var errorList = _parallelTestRunner.GetErrorsFromBeginTestIfAny(beginTest);

            Assert.IsTrue(errorList.Length == 0);

        }
    }
}

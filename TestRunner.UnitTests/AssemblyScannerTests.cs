using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using TestRunner.Framework.Concrete.Object;

namespace TestRunner.UnitTests
{
    [TestFixture]
    class AssemblyScannerTests
    {
        [Test]
        public void HappyPathGetTestsToRun()
        {
            var dllPath = @"C:\tfs\Fourth System\Release07\AutomationTests\Fourth.R9.Automation.Project\Fourth.R9.SmokeTests\bin\Debug\Fourth.R9.SmokeTests.dll";
            var projectName = "R9";
            var namesspaces = new List<string>()
            {
                "Fourth.R9.SmokeTests.HR.Employees.CreateEmployee",
                "Fourth.R9.SmokeTests.HR.Employees.EMPAudit"
            };

            var assemblyScanner = new AssemblyScanner();
            var testLibraryContainer = assemblyScanner.GetTestLibraryContainer(dllPath);
            var testsToRun = assemblyScanner.GetTestsToRun(testLibraryContainer, dllPath, projectName, namesspaces);
            testsToRun.Should().HaveCount(2);
        }

        [Test]
        public void NoNameSpacesGetTestsToRun()
        {
            var dllPath = @"C:\tfs\Fourth System\Release07\AutomationTests\Fourth.R9.Automation.Project\Fourth.R9.SmokeTests\bin\Debug\Fourth.R9.SmokeTests.dll";
            var projectName = "R9";
            List<string> namesspaces = null;

            var assemblyScanner = new AssemblyScanner();
            var testLibraryContainer = assemblyScanner.GetTestLibraryContainer(dllPath);
            var testsToRun = assemblyScanner.GetTestsToRun(testLibraryContainer, dllPath, projectName, namesspaces);
            testsToRun.Count.Should().BeGreaterThan(2);
        }
    }
}

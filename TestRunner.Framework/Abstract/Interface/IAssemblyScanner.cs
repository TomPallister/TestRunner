using System.Collections.Generic;
using TestRunner.Framework.Concrete.Model;

namespace TestRunner.Framework.Abstract.Interface
{
    public interface IAssemblyScanner
    {
        string[] CheckArguments(string[] strings);
        TestLibraryContainer GetTestLibraryContainer(string assemblyPath);
        List<TestToRun> GetTestsToRun(TestLibraryContainer testLibraryContainer, string assemblyLocation,
            string projectName, List<string> namesspaces);
    }
}

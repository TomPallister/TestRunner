using SimpleInjector;
using TestRunner.Framework.Abstract.Interface;

namespace TestRunner.Framework.Concrete.Object
{
    public static class IoC
    {
        public static Container Bind()
        {
            //set up dependency injection
            var container = new Container();
            container.Register<IAssemblyScanner, AssemblyScanner>();
            container.Register<IParallelTestRunner, ParallelTestRunner>();
            container.Register<ITestRunnerResultManager, TestResultManager>();
            container.Verify();
            return container;
        }
    }
}

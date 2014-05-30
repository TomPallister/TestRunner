using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using TestRunner.Framework.Abstract.Interface;
using TestRunner.Framework.Concrete.Model;

namespace TestRunner.Framework.Concrete.Object
{
    public class AssemblyScanner : IAssemblyScanner
    {
        #region Public Methods

        public string[] CheckArguments(string[] strings)
        {
            if (strings[0] != null && strings[1] != null && strings[2] != null)
            {
                return strings;
            }

            throw new Exception("The required command arguments were not passed in!");
        }

        public List<TypeDefinition> GetTypeDefinitions(ModuleDefinition moduleDefinition)
        {
            return moduleDefinition == null ? null : moduleDefinition.Types.ToList();
        }

        public List<TypeDefinition> GetTypesThatAreTestFixtures(List<TypeDefinition> typeDefinitions)
        {
            return typeDefinitions.Where(typeDefinition => typeDefinition.CustomAttributes.Count > 0 && 
                typeDefinition.CustomAttributes.FirstOrDefault
                    (x => x.AttributeType.FullName == "NUnit.Framework.TestFixtureAttribute") != null).ToList();
        }

        public List<TestClass> BuildTestClassesFromTestTypes(List<TypeDefinition> typeDefinitions)
        {
            return typeDefinitions.Select(typeDefinition => new TestClass
            {
                Name = typeDefinition.FullName,
            }).ToList();
        }

        public int GetAssertCountFromMethodDefinition(MethodDefinition methodDefinition)
        {
            return
                methodDefinition.Body.Instructions.Select(instruction => instruction.Operand as MemberReference)
                    .Count(memberReference => memberReference != null && (memberReference.DeclaringType != null
                                                                          &&
                                                                          (memberReference.DeclaringType.FullName
                                                                              .Contains("NUnit.Framework.Assert")
                                                                            ||
                                                                            memberReference.DeclaringType.FullName
                                                                               .Contains("FluentAssertions")
                                                                            ||
                                                                            memberReference.FullName
                                                                            .Contains("TechTalk.SpecFlow.ITestRunner::Then")
                                                                            )));
        }

        public bool IsSpecFlow(TypeDefinition typeDefinition)
        {
            if (typeDefinition.Fields.Count > 0)
            {
                return typeDefinition.Fields.Any(x => x.FullName.Contains("SpecFlow"));
            }

            return false;
        }

        public List<TestMethod> GetMethodsForTestClass(TestClass testClass, List<TypeDefinition> typeDefinitions)
        {

            var @class = testClass;
            var typeDefinition = typeDefinitions.FirstOrDefault(p => p.FullName == @class.Name);
            if (typeDefinition == null) return null;

            var methodDefinitions =
                typeDefinition.Methods.Where(
                    x => x.CustomAttributes.FirstOrDefault
                        (c => c.AttributeType.FullName == "NUnit.Framework.TestAttribute") != null);

            return methodDefinitions.Select(methodDefinition => new TestMethod
            {
                Name = methodDefinition.Name,
                AssertCount = GetAssertCountFromMethodDefinition(methodDefinition)
            }).ToList();


        }

        public TestLibraryContainer GetTestLibraryContainer(string assemblyPath)
        {
            var testLibraryContainer = new TestLibraryContainer();
            var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);

            var typeDefinitions = GetTypeDefinitions(assemblyDefinition.Modules.FirstOrDefault());
            var typesThatAreTestFixtures = GetTypesThatAreTestFixtures(typeDefinitions);

            testLibraryContainer.TestClasses.AddRange(BuildTestClassesFromTestTypes(typesThatAreTestFixtures));

            foreach (var testClass in testLibraryContainer.TestClasses)
            {
                testClass.TestMethods = GetMethodsForTestClass(testClass, typesThatAreTestFixtures);
            }

            return testLibraryContainer;
        }

        public List<TestToRun> GetTestsToRun(TestLibraryContainer testLibraryContainer, string assemblyLocation, string projectName, List<string> namespaces)
        {
            var testToRuns = new List<TestToRun>();

            if (namespaces != null)
            {
                foreach (var testClass in testLibraryContainer.TestClasses)
                {
                    foreach (var ns in namespaces)
                    {
                        if (testClass.Name.Contains(ns))
                        {
                            foreach (var testMethod in testClass.TestMethods)
                            {
                                var testToRun = new TestToRun();

                                var testRecord = new TestRecord
                                {
                                    AssertCount = testMethod.AssertCount,
                                    DateOfTest = DateTime.Now,
                                    Description = testMethod.Name,
                                    Executed = false,
                                    FullName = string.Format("{0}.{1}", testClass.Name, testMethod.Name),
                                    HasResults = false,
                                    IsError = false,
                                    IsFailure = false,
                                    IsSuccess = false,
                                    Message = "",
                                    Time = 0,
                                    Name = testMethod.Name,
                                    Project = projectName,
                                    StackTrace = "",
                                };

                                testToRun.TestRecord = testRecord;

                                var fixture = string.Format("/fixture:{0}", testClass.Name);
                                string[] arg = {fixture, assemblyLocation};

                                testToRun.Arguments = arg;

                                testToRuns.Add(testToRun);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var testClass in testLibraryContainer.TestClasses)
                {
                    foreach (var testMethod in testClass.TestMethods)
                    {
                        var testToRun = new TestToRun();

                        var testRecord = new TestRecord
                        {
                            AssertCount = testMethod.AssertCount,
                            DateOfTest = DateTime.Now,
                            Description = testMethod.Name,
                            Executed = false,
                            FullName = string.Format("{0}.{1}", testClass.Name, testMethod.Name),
                            HasResults = false,
                            IsError = false,
                            IsFailure = false,
                            IsSuccess = false,
                            Message = "",
                            Time = 0,
                            Name = testMethod.Name,
                            Project = projectName,
                            StackTrace = "",
                        };

                        testToRun.TestRecord = testRecord;

                        var fixture = string.Format("/fixture:{0}", testClass.Name);
                        string[] arg = {fixture, assemblyLocation};

                        testToRun.Arguments = arg;

                        testToRuns.Add(testToRun);
                    }
                }
            }

            var elements = new HashSet<string>(); // Type of property

            testToRuns.RemoveAll(i => !elements.Add(i.Arguments[0]));

            return testToRuns;
        }

        #endregion Public Methods
    }
}

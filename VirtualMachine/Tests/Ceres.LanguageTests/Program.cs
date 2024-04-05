using System.Linq;
using Ceres.ExecutionEngine;
using Ceres.LanguageTests;

var vm = new CeresVM();
vm.Start();

return await new TestRunner(
    vm, 
    new TestRunnerOptions(
        FailOnCompilationErrors: args.Contains("--fail-on-compiler-errors"),
        FailOnTestErrors: args.Contains("--fail-on-test-errors"),
        TestDirectoryRoot: "tests"
    )
).RunTests();

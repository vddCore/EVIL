using Ceres.ExecutionEngine;
using Insitor;

var vm = new CeresVM();
vm.Start();

var testRunner = new TestRunner("tests", vm);
await testRunner.RunTests(true);

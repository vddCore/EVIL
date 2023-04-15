using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ceres.ExecutionEngine;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.TranslationEngine;
using EVIL.Grammar.Parsing;
using Insitor;

async Task<(bool Success, DynamicValue Actual)> RunTest(CeresVM vm, Chunk chunk, DynamicValue expected)
{
    await vm.MainFiber.ScheduleAsync(chunk);
    var actual = vm.MainFiber.PopValue();
    return (expected.IsEqualTo(actual).IsTruth, actual);
}

var vm = new CeresVM();
vm.Run();
var testRunner = new TestRunner("tests", vm);
await testRunner.RunTests();

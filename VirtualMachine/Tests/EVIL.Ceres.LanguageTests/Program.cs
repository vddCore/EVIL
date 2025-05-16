using System.Collections.Generic;
using System.IO;

using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.LanguageTests;

var vm = new CeresVM();
vm.Run();

var failOnCompilerErrors = false;
var failOnTestErrors = false;
var optimizeCodeGeneration = false;
    
var argList = new List<string>(args);
for (var i = argList.Count - 1; i >= 0; i--)
{
    switch (argList[i])
    {
        case "--fail-on-compiler-errors":
        case "-fc":
        {
            failOnCompilerErrors = true;
            argList.RemoveAt(i);
            break;
        }

        case "--fail-on-test-errors":
        case "-ft":
        {
            failOnTestErrors = true;
            argList.RemoveAt(i);
            break;
        }

        case "--optimize-code-generation":
        case "-o":
        {
            optimizeCodeGeneration = true;
            argList.RemoveAt(i);
            break;
        }
    }
}

argList.RemoveAll(x => !Directory.Exists(x));

await new TestRunner(
    vm, 
    new TestRunnerOptions(
        FailOnCompilerErrors: failOnCompilerErrors,
        FailOnTestErrors: failOnTestErrors,
        OptimizeCodeGeneration: optimizeCodeGeneration,
        TestDirectories: argList.ToArray()
    )
).RunTests();

vm.Stop();

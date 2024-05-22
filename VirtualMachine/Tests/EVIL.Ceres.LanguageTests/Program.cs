using System.Collections.Generic;
using System.IO;
using System.Linq;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.LanguageTests;

var vm = new CeresVM();
vm.Start();

var failOnCompilerErrors = false;
var failOnTestErrors = false;
    
var argList = new List<string>(args);
for (var i = argList.Count - 1; i >= 0; i--)
{
    switch (argList[i])
    {
        case "--fail-on-compiler-errors":
        {
            failOnCompilerErrors = true;
            argList.RemoveAt(i);
            break;
        }

        case "--fail-on-test-errors":
        {
            failOnTestErrors = true;
            argList.RemoveAt(i);
            break;
        }
    }
}

argList.RemoveAll(x => !Directory.Exists(x));

return await new TestRunner(
    vm, 
    new TestRunnerOptions(
        FailOnCompilerErrors: failOnCompilerErrors,
        FailOnTestErrors: failOnTestErrors,
        TestDirectories: argList.ToArray()
    )
).RunTests();

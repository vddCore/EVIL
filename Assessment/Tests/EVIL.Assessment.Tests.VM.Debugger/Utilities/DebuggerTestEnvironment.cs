namespace EVIL.Assessment.Tests.VM.Debugger.Utilities;

using EVIL.VM.ExecutionEngine;
using EVIL.VM.Runtime;
using EVIL.VM.TranslationEngine;

internal static class DebuggerTestEnvironment
{
    public static (VirtualMachine VM, EvilRuntime Runtime, Compiler Compiler) Create()
    {
        var vm = new VirtualMachine();
        var runtime = new EvilRuntime(vm);
        runtime.RegisterBuiltInModules();
        runtime.RegisterBuiltInFunctions();
        var compiler = new Compiler();
        return (vm, runtime, compiler);
    }
}

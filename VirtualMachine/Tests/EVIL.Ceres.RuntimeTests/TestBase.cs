namespace EVIL.Ceres.RuntimeTests;

using System;
using System.Threading;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.TranslationEngine;
using EVIL.Grammar;
using NUnit.Framework;

public abstract class TestBase
{
    protected Parser Parser { get; private set; }
    protected Compiler Compiler { get; private set; }
    protected CeresVM VM { get; private set; }

    protected virtual void OnBeforeVirtualMachineInitialized()
    {
    }
    
    [SetUp]
    public virtual void Setup()
    {
        Parser = new Parser();
        Compiler = new Compiler();
        VM = new CeresVM();
        
        OnBeforeVirtualMachineInitialized();
        VM.Run();
    }
    
    [TearDown]
    public virtual void Teardown()
    {
        VM.Stop();
        
        Parser = null!;
        Compiler = null!;
    }
    
    protected DynamicValue EvilTestResult(string source, params DynamicValue[] args)
    {
        var chunk = Compiler.Compile(source, "!module_test_file!");
        var waitForCrashHandler = true;
            
        VM.MainFiber.Schedule(chunk);
        VM.MainFiber.Schedule(chunk["test"]!, false, args);
        VM.MainFiber.Resume();

        Exception? fiberException = null;
        VM.MainFiber.SetCrashHandler((f, e) =>
        {
            fiberException = e;
            waitForCrashHandler = false;
        });
            
        VM.MainFiber.BlockUntilFinished();

        while (waitForCrashHandler && VM.MainFiber.State != FiberState.Finished)
            Thread.Sleep(1);

        return VM.MainFiber.State switch
        {
            FiberState.Crashed => throw new Exception("Test has failed inside EVIL world.", fiberException),
            FiberState.Finished => VM.MainFiber.PopValue(),
            _ => throw new Exception("There is something wrong with the awaiter logic or the fiber itself.")
        };
    }
}
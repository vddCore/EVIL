namespace EVIL.Assessment.Tests.Base;

using System;
using System.Threading;
using EVIL.VirtualMachine.ExecutionEngine;
using EVIL.VirtualMachine.ExecutionEngine.Concurrency;
using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;
using EVIL.VirtualMachine.TranslationEngine;
using EVIL.Core.Grammar;
using NUnit.Framework;

public abstract class EvilTest
{
    protected Parser Parser { get; private set; } = null!;
    protected Compiler Compiler { get; private set; } = null!;
    protected VirtualMachine VM { get; private set; } = null!;

    protected virtual void OnBeforeVirtualMachineInitialized()
    {
    }
    
    [SetUp]
    public virtual void Setup()
    {
        Parser = new Parser();
        Compiler = new Compiler();
        VM = new VirtualMachine();
        
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
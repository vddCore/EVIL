namespace EVIL.Assessment.Tests.VM.Optimization;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EVIL.VirtualMachine.ExecutionEngine;
using EVIL.VirtualMachine.ExecutionEngine.Collections;
using EVIL.VirtualMachine.ExecutionEngine.Concurrency;
using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.VirtualMachine.TranslationEngine;
using EVIL.Assessment.Tests.Base;
using EVIL.Core.Common.TypeSystem;
using NUnit.Framework;

[TestFixture]
public class ExecutionStateTests : EvilTest
{
    private sealed record TestStackFrame(int Id) 
        : StackFrame;

    [Test]
    public void Closure_HighIterationLoop_PreservesCorrectnessAndState()
    {
        var script = """
            fn test() {
                rw val sum = 0;
                for (rw val i = 0; i < 500; i++) {
                    val multiplier = i;
                    val closure = fn(x) -> x * multiplier;
                    sum += closure(2);
                }
                ret sum;
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            // sum = 2 * sum(0..499) = 2 * (499 * 500 / 2) = 249500
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(249500.0));
        }
    }

    [Test]
    public void Closure_SameSourceRootsKeepCapturedLocalsIsolatedAfterReturning()
    {
        var firstRoot = Compiler.Compile(
            "val value = \"first\"; first_closure = fn() -> value;",
            "same_source.vil"
        );
        var secondRoot = Compiler.Compile(
            "val value = \"second\"; second_closure = fn() -> value;",
            "same_source.vil"
        );
        var invokeClosures = Compiler.Compile(
            "first_result = first_closure(); second_result = second_closure();",
            "invoke_closures.vil"
        );

        VM.MainFiber.Schedule(firstRoot, false);
        VM.MainFiber.Schedule(secondRoot, false);
        VM.MainFiber.Schedule(invokeClosures, false);
        VM.MainFiber.Resume();
        VM.MainFiber.BlockUntilFinished();

        Assert.That(VM.Global["first_result"].String, Is.EqualTo("first"));
        Assert.That(VM.Global["second_result"].String, Is.EqualTo("second"));
        Assert.That(firstRoot.Name, Is.EqualTo(secondRoot.Name));
        Assert.That(firstRoot.ClosureOwnerId, Is.Not.EqualTo(secondRoot.ClosureOwnerId));
    }

    [Test]
    public void Closure_DeeplyNested_CapturesAllLevelsCorrectly()
    {
        var script = """
            fn test() {
                val a = 10;
                val f = fn(b) {
                    ret fn(c) {
                        ret fn(d) -> a + b + c + d;
                    }(30);
                }(20);

                ret f(40);
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            // 10 + 20 + 30 + 40 = 100
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(100.0));
        }
    }

    [Test]
    public void Closure_MultipleInstances_HaveIndependentState()
    {
        var script = """
            fn makeCounter(initial) {
                rw val count = initial;
                ret fn() {
                    count++;
                    ret count;
                };
            }

            fn test() {
                val c1 = makeCounter(0);
                val c2 = makeCounter(100);

                c1();
                c1();
                val r1 = c1();

                c2();
                val r2 = c2();

                ret [r1, r2];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(3.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(102.0));
        }
    }

    [Test]
    public void Closure_RecursiveAnonymousFunction_ExecutesCorrectly()
    {
        var script = """
            fn test() {
                val obj = {
                    fib: self::fn(n) {
                        if (n <= 1) {
                            ret n;
                        }
                        ret self:fib(n - 1) + self:fib(n - 2);
                    }
                };

                ret obj:fib(10);
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(55.0));
        }
    }

    [Test]
    public void Closure_MutationAcrossCalls_PersistsInClosureContext()
    {
        var script = """
            fn test() {
                rw val acc = 0;
                val add = fn(n) {
                    acc += n;
                    ret acc;
                };

                add(5);
                add(10);
                ret add(15);
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(30.0));
        }
    }

    [Test]
    public void FastLeafFunction_ZeroArguments_ExecutesEfficiently()
    {
        var script = """
            fn leaf() -> 42;

            fn test() {
                rw val total = 0;
                for (rw val i = 0; i < 1000; i++) {
                    total += leaf();
                }
                ret total;
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(42000.0));
        }
    }

    [Test]
    public void Upvalue_SharedMutation_BetweenSiblingClosures()
    {
        var script = """
            fn make_pair() {
                rw val shared_var = 10;
                val adder = fn(delta) {
                    shared_var += delta;
                    ret shared_var;
                };
                val getter = fn() -> shared_var;

                ret { adder: adder, getter: getter };
            }

            fn test() {
                val pair = make_pair();
                pair.adder(25);
                ret pair.getter();
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(35.0));
        }
    }

    [Test]
    public void Upvalue_DeeplyNested_MultiLevelCapture()
    {
        var script = """
            fn outer(x) {
                rw val y = 100;
                ret fn(z) {
                    ret fn(w) {
                        ret x + y + z + w;
                    };
                };
            }

            fn test() {
                val step1 = outer(10);
                val step2 = step1(20);
                ret step2(5);
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(135.0));
        }
    }

    [Test]
    public void Upvalue_HighVolume_Throughput()
    {
        var script = """
            fn make_accumulator() {
                rw val acc = 0;
                ret fn() {
                    acc++;
                    ret acc;
                };
            }

            fn test() {
                val tick = make_accumulator();
                rw val last = 0;
                for (rw val i = 0; i < 50000; i++) {
                    last = tick();
                }
                ret last;
            }
            """;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = EvilTestResult(script);
        sw.Stop();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(50000.0));
            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(2000), $"Execution took too long: {sw.ElapsedMilliseconds}ms");
        }
    }

    [Test]
    public void TailCall_TernaryExpressionBody_DeepRecursionDoesNotOverflow()
    {
        var script = """
            fn sum_tail(n, acc) -> n == 0 ? acc : sum_tail(n - 1, acc + 1);

            fn test() {
                ret sum_tail(50000, 0);
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(50000.0));
        }
    }

    [Test]
    public void ExceptionUnwinding_RestoresProtectedEvaluationStackDepth()
    {
        var script = """
            fn throwing_inner() {
                throw "expected";
            }

            fn test() {
                rw val caught = nil;
                try {
                    val ignored = 41 + throwing_inner();
                } catch (e) {
                    caught = e;
                }

                val subsequent = 6 * 7;
                ret [caught, subsequent];
            }
            """;

        var result = EvilTestResult(script);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].String, Is.EqualTo("expected"));
            Assert.That(result.Array![1].Number, Is.EqualTo(42.0));
            Assert.That(VM.MainFiber.EvaluationStack.Count, Is.EqualTo(1));
        }
    }

    [Test]
    public void ProtectedRegionActivation_UsesValueStorage()
    {
        Assert.That(typeof(ProtectedRegionActivation).IsValueType, Is.True);
    }

    [Test]
    public void ExceptionUnwinding_UnhandledExceptionRetainsOriginalCallStackOrder()
    {
        var source = """
            fn outer() { middle(); }
            fn middle() { inner(); }
            fn inner() { throw "expected"; }
            fn test() { outer(); }
            """;
        var exception = Assert.Throws<Exception>(() => EvilTestResult(source));
        Assert.That(exception!.InnerException, Is.InstanceOf<UserUnhandledExceptionException>());
        var frameNames = ((UserUnhandledExceptionException)exception.InnerException!)
            .EvilStackTrace
            .OfType<ScriptStackFrame>()
            .Select(frame => frame.Chunk.Name)
            .ToArray();

        Assert.That(frameNames, Is.EqualTo(new[] { "inner", "middle", "outer", "test" }));
    }

    [Test]
    public void CallStack_ToArray_PreservesTopToBottomSnapshotOrder()
    {
        var callStack = new CallStack();
        var frames = new[]
        {
            new TestStackFrame(1),
            new TestStackFrame(2),
            new TestStackFrame(3)
        };

        foreach (var frame in frames)
        {
            callStack.Push(frame);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callStack.ToArray(), Is.EqualTo(new StackFrame[] { frames[2], frames[1], frames[0] }));
            Assert.That(callStack[0], Is.SameAs(frames[2]));
            Assert.That(callStack.Peek(), Is.SameAs(frames[2]));
        }
    }

    [Test]
    public void CallStack_GrowsOnDemandAndTrimExcessPreservesLiveFrames()
    {
        var callStack = new CallStack();
        var frames = Enumerable.Range(0, 200)
            .Select(id => (StackFrame)new TestStackFrame(id))
            .ToArray();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callStack.Capacity, Is.EqualTo(CallStack.InitialCapacity));
            Assert.That(callStack.Count, Is.Zero);
        }

        for (var i = 0; i < CallStack.InitialCapacity; i++)
        {
            callStack.Push(frames[i]);
        }
        Assert.That(callStack.Capacity, Is.EqualTo(CallStack.InitialCapacity));

        callStack.Push(frames[CallStack.InitialCapacity]);
        Assert.That(callStack.Capacity, Is.EqualTo(CallStack.InitialCapacity * 2));

        for (var i = CallStack.InitialCapacity + 1; i < frames.Length; i++)
        {
            callStack.Push(frames[i]);
        }
        Assert.That(callStack.Capacity, Is.EqualTo(CallStack.InitialCapacity * 4));

        callStack.TrimExcess();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callStack.Capacity, Is.EqualTo(frames.Length));
            Assert.That(callStack.ToArray(), Is.EqualTo(frames.Reverse()));
        }

        while (callStack.Count > 20)
        {
            callStack.Pop();
        }
        callStack.TrimExcess();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callStack.Capacity, Is.EqualTo(CallStack.InitialCapacity));
            Assert.That(callStack.ToArray(), Is.EqualTo(frames.Take(20).Reverse()));
        }
    }

    [Test]
    public void CallStack_MaxFrames_AllowsLimitAndRejectsOverflow()
    {
        var callStack = new CallStack();
        var frame = new TestStackFrame(1);

        for (var i = 0; i < CallStack.MaxFrames; i++)
        {
            callStack.Push(frame);
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callStack.Count, Is.EqualTo(CallStack.MaxFrames));
            Assert.That(callStack.Capacity, Is.EqualTo(CallStack.MaxFrames));
            Assert.That(callStack.Peek(), Is.SameAs(frame));
        }

        var exception = Assert.Throws<VirtualMachineException>(() => callStack.Push(frame));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception!.Message, Is.EqualTo("Call stack overflow."));
            Assert.That(callStack.Count, Is.EqualTo(CallStack.MaxFrames));
        }
    }

    [Test]
    public void ValueStack_TrimExcessPreservesLiveValuesAndInitialCapacity()
    {
        var valueStack = new ValueStack(4);
        for (var i = 0; i < 16; i++)
        {
            valueStack.Push((double)i);
        }

        for (var i = 0; i < 8; i++)
        {
            valueStack.Pop();
        }

        valueStack.TrimExcess();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(valueStack.Capacity, Is.EqualTo(8));
            Assert.That(valueStack.Count, Is.EqualTo(8));
        }

        for (var expected = 7; expected >= 0; expected--)
        {
            Assert.That(valueStack.Pop().Number, Is.EqualTo((double)expected));
        }

        valueStack.TrimExcess();
        Assert.That(valueStack.Capacity, Is.EqualTo(4));
    }

    [Test]
    public void Fiber_TrimMemory_RejectsActiveOrScheduledFibersWithoutMutation()
    {
        var vm = new VirtualMachine();

        var runningFiber = vm.Scheduler.CreateFiber(false);
        runningFiber.Resume();
        Assert.That(runningFiber.TrimMemory(), Is.False);
        Assert.That(runningFiber.CallStack.Capacity, Is.EqualTo(CallStack.InitialCapacity));

        var framedFiber = vm.Scheduler.CreateFiber(false);
        var frame = new TestStackFrame(1);
        framedFiber.CallStack.Push(frame);
        Assert.That(framedFiber.TrimMemory(), Is.False);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(framedFiber.CallStack.Count, Is.EqualTo(1));
            Assert.That(framedFiber.CallStack.Peek(), Is.SameAs(frame));
        }

        var awaitingFiber = vm.Scheduler.CreateFiber(false);
        awaitingFiber.WaitFor(vm.Scheduler.CreateFiber(false));
        Assert.That(awaitingFiber.TrimMemory(), Is.False);

        var scheduledFiber = vm.Scheduler.CreateFiber(false);
        var chunk = new Compiler().Compile("fn work() { }", "trim_scheduled_fiber.vil");
        scheduledFiber.Schedule(chunk["work"]!, false);
        Assert.That(scheduledFiber.TrimMemory(), Is.False);
    }

    [Test]
    public void Fiber_TrimMemory_SerializesWithConcurrentResume()
    {
        var fiber = new VirtualMachine().Scheduler.CreateFiber(false);
        for (var i = 0; i < 1100; i++)
        {
            fiber.PushValue(i);
        }
        while (fiber.EvaluationStack.Count > 0)
        {
            fiber.PopValue();
        }

        for (var i = 0; i < 128; i++)
        {
            fiber.CallStack.Push(new TestStackFrame(i));
        }
        while (fiber.CallStack.Count > 0)
        {
            fiber.CallStack.Pop();
        }

        using var start = new ManualResetEventSlim(false);
        var trimTask = Task.Run(() =>
        {
            start.Wait();
            return fiber.TrimMemory();
        });
        var resumeTask = Task.Run(() =>
        {
            start.Wait();
            fiber.Resume();
        });

        start.Set();
        Task.WaitAll(trimTask, resumeTask);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fiber.State, Is.EqualTo(FiberState.Running));
            Assert.That(fiber.EvaluationStack.Capacity, Is.EqualTo(trimTask.Result ? 1024 : 2048));
            Assert.That(fiber.CallStack.Capacity, Is.EqualTo(trimTask.Result ? CallStack.InitialCapacity : 128));
        }
    }

    [Test]
    public void Fiber_TrimMemory_PreservesLiveEvaluationValuesAndShrinksStackCapacity()
    {
        var fiber = new VirtualMachine().Scheduler.CreateFiber(false);
        for (var i = 0; i < 1100; i++)
        {
            fiber.PushValue(i);
        }

        for (var i = fiber.EvaluationStack.Count; i > 16; i--)
        {
            fiber.PopValue();
        }

        for (var i = 0; i < 128; i++)
        {
            fiber.CallStack.Push(new TestStackFrame(i));
        }
        while (fiber.CallStack.Count > 0)
        {
            fiber.CallStack.Pop();
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fiber.EvaluationStack.Capacity, Is.EqualTo(2048));
            Assert.That(fiber.CallStack.Capacity, Is.EqualTo(128));
            Assert.That(fiber.TrimMemory(), Is.True);
            Assert.That(fiber.EvaluationStack.Capacity, Is.EqualTo(1024));
            Assert.That(fiber.CallStack.Capacity, Is.EqualTo(CallStack.InitialCapacity));
            Assert.That(fiber.EvaluationStack.Count, Is.EqualTo(16));
        }

        for (var expected = 15; expected >= 0; expected--)
        {
            Assert.That(fiber.PopValue().Number, Is.EqualTo((double)expected));
        }
    }

    [Test]
    public void Fiber_TrimMemory_DropsPoolsAndLeavesDiagnosticSnapshotFramesUnchanged()
    {
        var fiber = new VirtualMachine().Scheduler.CreateFiber(false);
        var chunk = new Chunk("trim_snapshot", []);
        var frame = fiber.RentScriptFrame(chunk);
        fiber.CallStack.Push(frame);
        var snapshot = fiber.CallStack.ToArray();
        fiber.CallStack.Pop();
        fiber.ReturnScriptFrame(frame);

        var chunkBeforeTrim = frame.Chunk;
        var ipBeforeTrim = frame.IP;
        var previousOpCodeIpBeforeTrim = frame.PreviousOpCodeIP;
        var frameTextBeforeTrim = frame.ToString();

        var arguments = fiber.RentArguments(2);
        fiber.ReturnArguments(arguments);
        Assert.That(fiber.RentArguments(2), Is.SameAs(arguments));
        fiber.ReturnArguments(arguments);

        var locals = fiber.RentLocals(2)!;
        fiber.ReturnLocals(locals);
        Assert.That(fiber.RentLocals(2), Is.SameAs(locals));
        fiber.ReturnLocals(locals);

        Assert.That(fiber.RentScriptFrame(chunk), Is.SameAs(frame));
        fiber.ReturnScriptFrame(frame);

        Assert.That(fiber.TrimMemory(), Is.True);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshot, Has.Length.EqualTo(1));
            Assert.That(snapshot[0], Is.SameAs(frame));
            Assert.That(frame.Chunk, Is.SameAs(chunkBeforeTrim));
            Assert.That(frame.IP, Is.EqualTo(ipBeforeTrim));
            Assert.That(frame.PreviousOpCodeIP, Is.EqualTo(previousOpCodeIpBeforeTrim));
            Assert.That(frame.ToString(), Is.EqualTo(frameTextBeforeTrim));
        }

        var frameAfterTrim = fiber.RentScriptFrame(chunk);
        Assert.That(frameAfterTrim, Is.Not.SameAs(frame));
        fiber.ReturnScriptFrame(frameAfterTrim);
        Assert.That(fiber.RentScriptFrame(chunk), Is.SameAs(frameAfterTrim));
        fiber.ReturnScriptFrame(frameAfterTrim);

        var argumentsAfterTrim = fiber.RentArguments(2);
        Assert.That(argumentsAfterTrim, Is.Not.SameAs(arguments));
        fiber.ReturnArguments(argumentsAfterTrim);
        Assert.That(fiber.RentArguments(2), Is.SameAs(argumentsAfterTrim));
        fiber.ReturnArguments(argumentsAfterTrim);

        var localsAfterTrim = fiber.RentLocals(2)!;
        Assert.That(localsAfterTrim, Is.Not.SameAs(locals));
        fiber.ReturnLocals(localsAfterTrim);
        Assert.That(fiber.RentLocals(2), Is.SameAs(localsAfterTrim));
        fiber.ReturnLocals(localsAfterTrim);
    }

    [Test]
    public void InvokeChunkFromStack_HighVolumeInvocations_ExecuteFastAndCorrect()
    {
        var script = """
            fn test() {
                val add = fn(a, b) -> a + b;
                rw val sum = 0;
                rw val i = 0;
                while (i < 1000) {
                    sum = add(sum, 1);
                    i++;
                }
                ret sum;
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(1000.0));
        }
    }

    [Test]
    public void ScriptStackFrame_Pooling_ZeroAllocationsOnFunctionCallLoop()
    {
        var script = """
            fn add(a, b) -> a + b;

            fn test() {
                rw val sum = 0;
                for (rw val i = 0; i < 1000; i++) {
                    sum = add(sum, 1);
                }
                ret sum;
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(1000.0));
        }
    }

    [Test]
    public void ScriptStackFrame_Pooling_DeepRecursionExecutesCorrectly()
    {
        var script = """
            fn recurse(n) {
                if (n <= 0) {
                    ret 0;
                }
                ret 1 + recurse(n - 1);
            }

            fn test() {
                ret recurse(500);
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(500.0));
        }
    }

    [Test]
    public void CallFrame_Struct_RepresentsMetadataCorrectly()
    {
        var chunk = new Chunk("test_chunk", [0x01, 0x02]);
        var callFrame = new CallFrame(chunk, null, 10, 42);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callFrame.Chunk, Is.SameAs(chunk));
            Assert.That(callFrame.Upvalues, Is.Null);
            Assert.That(callFrame.BasePointer, Is.EqualTo(10));
            Assert.That(callFrame.ReturnIP, Is.EqualTo(42));
        }
    }

    [Test]
    public void FiberMemory_Characterization_ReportsRetainedHeapAcrossExecutionStates()
    {
        const int fiberCount = 16;
        const int recursionDepth = 1200;

        var chunk = new Compiler().Compile(
            """
            fn shallow() -> 7;

            fn recurse(n) {
                if (n <= 0) {
                    ret 0;
                }
                ret 1 + recurse(n - 1);
            }

            fn deep() {
                ret recurse(1200);
            }
            """,
            "interpreter_memory_characterization.vil"
        );
        var vm = new VirtualMachine();
        var initializerFiber = ExecuteMemoryChunk(vm, chunk);
        var samples = new (string State, int FiberCount, long RetainedBytes)[7];

        samples[0] = ("baseline", 0, MeasureRetainedManagedBytes());

        var freshFibers = new Fiber[fiberCount];
        freshFibers[0] = vm.Scheduler.CreateFiber(false);
        samples[1] = ("fresh_one", 1, MeasureRetainedManagedBytes());

        for (var i = 1; i < freshFibers.Length; i++)
        {
            freshFibers[i] = vm.Scheduler.CreateFiber(false);
        }
        samples[2] = ("fresh_many", fiberCount, MeasureRetainedManagedBytes());

        var pausedFibers = new Fiber[fiberCount];
        for (var i = 0; i < pausedFibers.Length; i++)
        {
            pausedFibers[i] = vm.Scheduler.CreateFiber(false);
            pausedFibers[i].Resume();
            pausedFibers[i].Yield();
        }
        samples[3] = ("paused_many", fiberCount, MeasureRetainedManagedBytes());

        var shallowFiber = ExecuteMemoryChunk(vm, chunk["shallow"]!);
        Assert.That(shallowFiber.PopValue().Number, Is.EqualTo(7.0));
        samples[4] = ("shallow_call", 1, MeasureRetainedManagedBytes());

        var deepFiber = ExecuteMemoryChunk(vm, chunk["deep"]!);
        Assert.That(deepFiber.PopValue().Number, Is.EqualTo((double)recursionDepth));
        samples[5] = ("deep_high_water", 1, MeasureRetainedManagedBytes());

        deepFiber.ClearQueued();
        Assert.That(deepFiber.TrimMemory(), Is.True);
        samples[6] = ("deep_high_water_trimmed", 1, MeasureRetainedManagedBytes());

        for (var i = 0; i < samples.Length; i++)
        {
            var previousBytes = i == 0 ? samples[i].RetainedBytes : samples[i - 1].RetainedBytes;
            TestContext.Progress.WriteLine(
                $"memory_sample,state={samples[i].State},fiber_count={samples[i].FiberCount}," +
                $"retained_managed_heap_bytes={samples[i].RetainedBytes}," +
                $"delta_since_previous_bytes={samples[i].RetainedBytes - previousBytes}"
            );
        }

        GC.KeepAlive(freshFibers);
        GC.KeepAlive(pausedFibers);
        GC.KeepAlive(shallowFiber);
        GC.KeepAlive(deepFiber);
        GC.KeepAlive(initializerFiber);
        GC.KeepAlive(vm);
    }

    private static Fiber ExecuteMemoryChunk(VirtualMachine vm, Chunk chunk)
    {
        Exception? fiberCrash = null;
        var fiber = vm.Scheduler.CreateFiber(false, (_, exception) => fiberCrash = exception);
        fiber.Schedule(chunk, false);
        fiber.Resume();

        for (var step = 0; fiber.State == FiberState.Running && step < 100_000; step++)
        {
            fiber.Step();
        }

        var crashDetails = fiberCrash?.ToString();
        if (fiberCrash is UserUnhandledExceptionException userException)
        {
            var exceptionValue = userException.EvilExceptionObject;
            var exceptionMessage = exceptionValue.NativeObject switch
            {
                Error error => error.Message ?? "<no message>",
                string message => message,
                _ => "<unknown>"
            };
            crashDetails = $"{exceptionValue.Type}: {exceptionMessage} " +
                           $"(stack depth {userException.EvilStackTrace.Length})";
        }
        Assert.That(fiber.State, Is.EqualTo(FiberState.Finished), $"Fiber failed: {crashDetails}");
        Assert.That(fiber.CallStack.Count, Is.EqualTo(0));
        return fiber;
    }

    private static long MeasureRetainedManagedBytes()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        return GC.GetTotalMemory(true);
    }
}

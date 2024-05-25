using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

namespace EVIL.Ceres.LanguageTests
{
    public class Test
    {
        private readonly Chunk _chunk;
        private bool _processingCrash;

        public Fiber Fiber { get; }

        public bool CallsAnyAsserts { get; private set; }

        public bool Successful { get; private set; } = true;
        public string ErrorMessage { get; private set; } = string.Empty;
        public List<string> StackTrace { get; private set; } = new();

        public Test(CeresVM vm, Chunk chunk)
        {
            _chunk = chunk;
            Fiber = vm.Scheduler.CreateFiber(
                true,
                TestCrashHandler,
                (Dictionary<string, ClosureContext>)vm.MainFiber.ClosureContexts
            );

            Fiber.SetOnNativeFunctionInvoke(OnNativeFunctionInvoke);
        }

        public async Task Run()
        {
            Fiber.Schedule(_chunk);

            while (Fiber.State != FiberState.Crashed
                   && Fiber.State != FiberState.Finished)
            {
                await Task.Delay(TimeSpan.FromMicroseconds(1));
            }

            if (Fiber.State == FiberState.Crashed)
            {
                _processingCrash = true;
            }
        }

        public async Task WaitForCleanup()
        {
            while (_processingCrash)
            {
                await Task.Delay(TimeSpan.FromMicroseconds(1));
            }

            Fiber.DeImmunize();
        }

        private void TestCrashHandler(Fiber fiber, Exception exception)
        {
            Successful = false;

            if (exception is UserUnhandledExceptionException uuee)
            {
                if (uuee.EvilExceptionObject.Type == DynamicValueType.Error)
                {
                    var e = uuee.EvilExceptionObject.Error!;
                    if (e["__should_have_thrown"] == true)
                    {
                        CallsAnyAsserts = true;

                        if (e["__threw"] == true)
                        {
                            Successful = true;
                        }
                        else
                        {
                            ErrorMessage = "Expected function to throw, but it was successful.";
                        }
                    }
                    else if (e["__should_not_have_thrown"] != DynamicValue.Nil)
                    {
                        CallsAnyAsserts = true;

                        if (e["__threw"] == false)
                        {
                            Successful = true;
                        }
                        else
                        {
                            ErrorMessage = "Expected function to be successful, but it threw.";
                        }
                    }
                }
            }
            else
            {
                ErrorMessage = exception.Message;
            }

            if (!Successful)
            {
                StackTrace.AddRange(fiber.StackTrace(false).Split('\n').Where(x => !string.IsNullOrEmpty(x)));
            }

            _processingCrash = false;
        }

        private void OnNativeFunctionInvoke(Fiber fiber, NativeFunction nativeFunction)
        {
            var nativeFunctionType = nativeFunction.Method.DeclaringType;
            if (nativeFunctionType == null) return;

            if (nativeFunctionType.IsAssignableTo(typeof(AssertModule)))
            {
                CallsAnyAsserts = true;
            }
        }
    }
}
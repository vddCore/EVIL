using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics;

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
                await Task.Delay(1);
            }

            while (_processingCrash)
            {
                await Task.Delay(1);
            }
            
            Fiber.DeImmunize();
        }

        private void TestCrashHandler(Fiber fiber, Exception exception)
        {
            _processingCrash = true;

            Successful = false;
            ErrorMessage = exception.Message;
            StackTrace.AddRange(fiber.StackTrace(false).Split('\n').Where(x => !string.IsNullOrEmpty(x)));
            
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
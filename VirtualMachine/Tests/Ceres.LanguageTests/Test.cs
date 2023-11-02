using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ceres.ExecutionEngine;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;

namespace Ceres.LanguageTests
{
    public class Test
    {
        private readonly Chunk _chunk;
        private bool _processingCrash;
        
        public Fiber Fiber { get; }

        public bool Successful { get; private set; } = true;
        public string ErrorMessage { get; private set; } = string.Empty;
        public List<string> StackTrace { get; private set; } = new();

        public Test(CeresVM vm, Chunk chunk)
        {
            _chunk = chunk;
            Fiber = vm.Scheduler.CreateFiber(true, TestCrashHandler);
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
    }
}
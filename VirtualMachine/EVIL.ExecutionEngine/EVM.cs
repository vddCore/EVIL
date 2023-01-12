using System;
using System.Collections.Generic;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Diagnostics;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.ExecutionEngine
{
    public class EVM
    {
        private Executable Executable { get; set; }
        private List<ExecutionContext> ExecutionContexts { get; } = new();

        public ExecutionContext MainExecutionContext => ExecutionContexts[0];
        public List<string> ImportLookupPaths { get; set; } = new();

        public bool Running { get; private set; }
        public Table GlobalTable { get; }

        public EVM(Table globalTable)
        {
            GlobalTable = globalTable ?? new Table();
            ExecutionContexts.Add(new ExecutionContext(this));
            ImportLookupPaths.Add(AppContext.BaseDirectory);
        }

        public void ImportPublicFunctions(Executable executable)
        {
            for (var i = 1; i < executable.Chunks.Count; i++)
            {
                var c = executable.Chunks[i];

                if (!c.IsPublic)
                    continue;

                GlobalTable.Set(
                    new DynamicValue(c.Name),
                    new DynamicValue(c)
                );
            }
        }

        public void Load(Executable executable)
        {
            if (Running)
                Stop();

            Executable = executable;

            ImportPublicFunctions(executable);
        }

        public void Run(params DynamicValue[] args)
        {
            InvokeCallback(
                Executable.RootChunk, 
                MainExecutionContext, 
                args
            );
            
            Start();
        }
        
        public void Start()
        {
            Running = true;

            while (Running)
            {
                var isAnyContextActive = false;
                for (var i = 0; i < ExecutionContexts.Count; i++)
                {
                    if (!Running)
                        break;

                    var ctx = ExecutionContexts[i];

                    if (ctx.Running)
                    {
                        isAnyContextActive = true;
                        ctx.Step();
                    }
                }

                if (!isAnyContextActive)
                {
                    Running = false;
                }
            }
        }

        public void Stop()
        {
            for (var i = 0; i < ExecutionContexts.Count; i++)
                ExecutionContexts[i].Pause();
        }

        public void SetGlobal(string key, DynamicValue value)
        {
            GlobalTable.Set(new(key), value);
        }

        public void SetGlobal(double key, DynamicValue value)
        {
            GlobalTable.Set(new(key), value);
        }

        public ExecutionContext CreateNewExecutionContext()
        {
            var ctxt = new ExecutionContext(this);
            ExecutionContexts.Add(ctxt);

            return ctxt;
        }

        public void DeleteExecutionContext(ExecutionContext ctx)
        {
            if (ctx == MainExecutionContext)
            {
                throw new InvalidOperationException("Unable to delete main execution context.");
            }
            
            ctx.Pause();
            ExecutionContexts.Remove(ctx);
        }

        public void InvokeCallback(Chunk chunk, ExecutionContext ctx, params DynamicValue[] args)
        {
            if (ctx == null)
                ctx = MainExecutionContext;
            
            if (!ExecutionContexts.Contains(ctx))
                throw new InvalidOperationException("Execution context provided is not owned by this EVM.");

            if (chunk == null)
                throw new InvalidOperationException($"Chunk was null.");

            if (args.Length > 255)
                throw new InvalidOperationException("Too many arguments passed to the chunk (max. 255).");

            ctx.ScheduleChunk(chunk, args);
        }

        public Chunk FindExposedChunk(string funcName)
        {
            Chunk c = null;
            for (var i = 0; i < Executable.Chunks.Count; i++)
            {
                var ch = Executable.Chunks[i];

                if (ch.Name.StartsWith("!"))
                    continue;

                if (ch.Name == funcName)
                {
                    c = ch;
                    break;
                }
            }

            return c;
        }
    }
}
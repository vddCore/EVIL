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

        public List<ExecutionContext> ExecutionContexts { get; } = new();
        public ExecutionContext MainExecutionContext { get; private set; }

        public bool Running { get; private set; }
        public Table GlobalTable { get; }

        public EVM(Table globalTable)
        {
            GlobalTable = globalTable ?? new Table();
        }

        public void Load(Executable executable)
        {
            if (Running)
                Stop();

            Executable = executable;

            for (var i = 0; i < executable.Globals.Count; i++)
            {
                var name = executable.Globals[i];

                GlobalTable.Set(
                    new DynamicValue(name),
                    DynamicValue.Zero
                );
            }

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

        public void Start()
        {
            Running = true;

            while (Running)
            {
                if (ExecutionContexts.Count <= 0)
                {
                    Running = false;
                    continue;
                }
                
                for (var i = 0; i < ExecutionContexts.Count; i++)
                {
                    if (!Running)
                        break;
                    
                    var ctx = ExecutionContexts[i];

                    if (ctx.Running)
                    {
                        ctx.Step();
                    }
                    else
                    {
                        ExecutionContexts.RemoveAt(i);
                    }
                }
            }

            ExecutionContexts.Clear();
        }

        public void Stop()
        {
            for (var i = 0; i < ExecutionContexts.Count; i++)
                ExecutionContexts[i].Pause();

            Running = false;
        }

        public void SetGlobal(string key, DynamicValue value)
        {
            GlobalTable.Set(new(key), value);
        }

        public void SetGlobal(double key, DynamicValue value)
        {
            GlobalTable.Set(new(key), value);
        }

        public ExecutionContext InvokeCallback(Chunk chunk, params DynamicValue[] args)
        {
            var exeContext = new ExecutionContext(this, chunk, args);

            if (chunk == null)
                throw new InvalidOperationException($"Chunk was null.");

            if (args.Length > 255)
                throw new InvalidOperationException("Too many arguments passed to the chunk (max. 255).");

            ExecutionContexts.Add(exeContext);
            exeContext.Resume();

            return exeContext;
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

        public void Run(params DynamicValue[] args)
        {
            MainExecutionContext = InvokeCallback(Executable.RootChunk, args);
            Start();
        }
    }
}
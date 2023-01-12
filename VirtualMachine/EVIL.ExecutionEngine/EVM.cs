using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Diagnostics;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.ExecutionEngine
{
    public class EVM
    {
        private List<ExecutionContext> ExecutionContexts { get; } = new();
        public ExecutionContext MainExecutionContext => ExecutionContexts[0];

        public bool Running { get; private set; }
        public Table GlobalTable { get; private set; }

        public EVM()
        {
            ResetGlobalTable();
            ResetExecutionContexts();
        }

        public EVM(Table globalTable)
        {
            if (globalTable == null)
            {
                throw new ArgumentNullException(
                    nameof(globalTable), 
                    "A global table is required."
                );
            }

            GlobalTable = globalTable;
            ResetExecutionContexts();
        }

        public void Reset(bool preserveGlobals = false)
        {
            Stop();
            ResetExecutionContexts();

            if (!preserveGlobals)
            {
                ResetGlobalTable();
            }
        }

        public void RunExecutable(Executable executable, params DynamicValue[] args)
        {
            Reset(true);

            foreach (var c in executable.Chunks.Where(x => x.IsPublic))
                GlobalTable.Set(c.Name, c);

            this.SetEnvironmentVariable(
                EvilEnvironmentVariable.ExecutableArguments,
                new Table(args)
            );

            RunChunk(executable.RootChunk);
        }

        public void SetEnvironmentVariable(EvilEnvironmentVariable var, DynamicValue value)
            => GlobalTable.Set(var.Name, value);

        public void RunChunk(Chunk chunk, ExecutionContext ctx = null, params DynamicValue[] args)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk), "Need a chunk to run it, don't you think?");

            ctx ??= MainExecutionContext;

            InvokeCallback(chunk, ctx, args);
            Start();
        }

        public void Start()
        {
            if (Running)
                return;

            Running = true;
            {
                while (Running)
                {
                    lock (ExecutionContexts)
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
                            break;
                        }
                    }
                }
            }
            Running = false;
        }

        public void Stop()
        {
            lock (ExecutionContexts)
            {
                for (var i = 0; i < ExecutionContexts.Count; i++)
                    ExecutionContexts[i].Halt();
            }
        }

        public ExecutionContext CreateNewExecutionContext()
        {
            var ctxt = new ExecutionContext(ExecutionContexts.Count, this);

            lock (ExecutionContexts)
            {
                ExecutionContexts.Add(ctxt);
            }

            return ctxt;
        }

        public void DeleteExecutionContext(ExecutionContext ctx)
        {
            if (ctx == MainExecutionContext)
            {
                throw new InvalidOperationException("Unable to delete main execution context.");
            }

            ctx.Halt();

            lock (ExecutionContexts)
            {
                ExecutionContexts.Remove(ctx);
            }
        }

        public void InvokeCallback(Chunk chunk, ExecutionContext ctx, params DynamicValue[] args)
        {
            if (ctx == null)
                ctx = MainExecutionContext;

            lock (ExecutionContexts)
            {
                if (!ExecutionContexts.Contains(ctx))
                    throw new InvalidOperationException("Execution context provided is not owned by this EVM.");
            }

            if (chunk == null)
                throw new InvalidOperationException("Chunk was null.");

            if (args.Length > 255)
                throw new InvalidOperationException("Too many arguments passed to the chunk (max. 255).");

            ctx.ScheduleChunk(chunk, args);
        }

        public string DumpAllExecutionContexts()
        {
            var sb = new StringBuilder();

            lock (ExecutionContexts)
            {
                for (var i = 0; i < ExecutionContexts.Count; i++)
                {
                    var ec = ExecutionContexts[i];

                    if (ec.CallStack.TryPeek(out var frame))
                    {
                        sb.AppendLine($"--- === EC {i} : {frame.Chunk.Name} @ {frame.IP:X8} === ---");
                    }
                    else
                    {
                        sb.AppendLine($"--- === EC {i} (inactive) === ---");
                    }

                    sb.AppendLine("--- [call stack] ---");
                    sb.AppendLine(ec.DumpCallStack());
                    sb.AppendLine();
                    sb.AppendLine("--- [evaluation stack] ---");
                    sb.AppendLine(ec.DumpEvaluationStack());
                    sb.AppendLine($"--- === EC {i} === ---");
                }
            }

            return sb.ToString();
        }

        public void SetGlobalDefaults()
        {
            GlobalTable.Set("_G", GlobalTable);

            this.SetEnvironmentVariable(
                EvilEnvironmentVariable.CurrentWorkingDirectory,
                Environment.CurrentDirectory
            );

            this.SetEnvironmentVariable(
                EvilEnvironmentVariable.LibraryLookupPaths,
                "?.vil;?/?.vil"
            );
        }

        private void ResetGlobalTable()
        {
            GlobalTable = new Table();
        }

        private void ResetExecutionContexts()
        {
            lock (ExecutionContexts)
            {
                ExecutionContexts.Clear();
                ExecutionContexts.Add(new ExecutionContext(0, this));
            }
        }
    }
}
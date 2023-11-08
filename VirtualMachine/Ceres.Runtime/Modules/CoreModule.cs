using System;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;
using static EVIL.CommonTypes.TypeSystem.DynamicValueType;
using EvilArray = Ceres.ExecutionEngine.Collections.Array;

namespace Ceres.Runtime.Modules
{
    public sealed class CoreModule : RuntimeModule
    {
        public override string FullyQualifiedName => "core";

        [RuntimeModuleFunction("gc.mem.get_used_total")]
        [EvilDocFunction(
            "Attempts to retrieve the amount of managed memory currently thought to be allocated.",
            Returns = "The amount of managed memory - in bytes - currently thought to be allocated.",
            ReturnType = Number
        )]
        [EvilDocArgument(
            "force_full_collection",
            "`true` to indicate waiting for GC, `false` to return an immediate value.",
            DynamicValueType.Boolean,
            DefaultValue = "false"
        )]
        private static DynamicValue GcMemGetTotal(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtMost(1);
            args.OptionalBooleanAt(0, false, out var forceFullCollection);
            
            return GC.GetTotalMemory(forceFullCollection);
        }

        [RuntimeModuleFunction("gc.mem.get_info")]
        [EvilDocFunction(
            "Retrieves memory statistics of the garbage collector.",
            Returns = "A table containing information about GC memory statistics.",
            ReturnType = DynamicValueType.Table
        )]
        private static DynamicValue GcMemGetInfo(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNone();
            
            var info = GC.GetGCMemoryInfo();

            return new Table
            {
                ["index"] = info.Index,
                ["generation"] = info.Generation,
                ["is_compacted"] = info.Compacted,
                ["is_concurrent"] = info.Concurrent,
                ["finalization_pending_count"] = info.FinalizationPendingCount,
                ["fragmented_bytes"] = info.FragmentedBytes,
                ["promoted_bytes"] = info.PromotedBytes,
                ["heap_size_bytes"] = info.HeapSizeBytes,
                ["total_committed_bytes"] = info.TotalCommittedBytes,
                ["total_avail_mem_bytes"] = info.TotalAvailableMemoryBytes,
                ["pause_time_percent"] = info.PauseTimePercentage,
                ["pinned_object_count"] = info.PinnedObjectsCount
            };
        }
        
        [RuntimeModuleFunction("gc.collect")]
        [EvilDocFunction("Forces a compacting asynchronous aggressive garbage collection for all generations.")]
        private static DynamicValue GcCollect(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNone();
            GC.Collect(
                GC.MaxGeneration, 
                GCCollectionMode.Aggressive, 
                false, 
                true
            );

            return DynamicValue.Nil;
        }

        [RuntimeModuleFunction("strace")]
        [EvilDocFunction(
            "Gets the current stack trace in the form of raw data.",
            Returns = "Array containing tables with stack frame information available at the time of invocation.",
            ReturnType = DynamicValueType.Array
        )]
        [EvilDocArgument(
            "skip_native_frames",
            "`true` to skip native invocation frames, `false` to include them in the output.",
            DynamicValueType.Boolean,
            DefaultValue = "false"
        )]
        private static DynamicValue StackTrace(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectAtMost(1)
                .OptionalBooleanAt(0, defaultValue: false, out var skipNativeFrames);
            
            var callStack = fiber.CallStack.ToArray(skipNativeFrames);
            var array = new EvilArray(callStack.Length);
            
            for (var i = 0; i < callStack.Length; i++)
            {
                if (callStack[i] is ScriptStackFrame ssf)
                {
                    array[i] = new Table
                    {
                        { "is_script", true },
                        { "fn_name", ssf.Chunk.Name ?? "<unknown>" },
                        { "ip", ssf.IP },
                        { "line", ssf.Chunk.HasDebugInfo ? ssf.Chunk.DebugDatabase.GetLineForIP((int)ssf.PreviousOpCodeIP) : DynamicValue.Nil },
                        { "locals", ssf.Locals?.ToTable() ?? DynamicValue.Nil },
                        { "args", ssf.Arguments.ToTable() },
                        { "xargs", ssf.ExtraArguments.DeepCopy() },
                        { "def_on_line", ssf.Chunk.DebugDatabase.DefinedOnLine > 0 ? ssf.Chunk.DebugDatabase.DefinedOnLine : DynamicValue.Nil },
                        { "def_in_file", !string.IsNullOrEmpty(ssf.Chunk.DebugDatabase.DefinedInFile) ? ssf.Chunk.DebugDatabase.DefinedInFile : DynamicValue.Nil }
                    };
                }
                else if (callStack[i] is NativeStackFrame nsf && !skipNativeFrames)
                {
                    array[i] = new Table
                    {
                        { "is_script", false },
                        { "fn_name", nsf.NativeFunction.Method.Name },
                        { "decl_type", nsf.NativeFunction.Method.DeclaringType!.FullName! },
                    };
                }
            }
            
            return array;
        }

        [RuntimeModuleFunction("strace_s")]
        [EvilDocFunction(
            "Gets the current stack trace as a formatted string.",
            Returns = "Formatted string containing the stack trace available at the time of the invocation.",
            ReturnType = DynamicValueType.String
        )]
        [EvilDocArgument(
            "skip_native_frames",
            "`true` to skip native invocation frames, `false` to include them in the output.",
            DynamicValueType.Boolean,
            DefaultValue = "false"
        )]
        private static DynamicValue StackTraceString(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectAtMost(1)
                .OptionalBooleanAt(0, defaultValue: false, out var skipNativeFrames);

            return fiber.StackTrace(skipNativeFrames);
        }
    }
}
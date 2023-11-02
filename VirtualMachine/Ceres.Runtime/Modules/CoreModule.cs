using System;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;
using Array = Ceres.ExecutionEngine.Collections.Array;

namespace Ceres.Runtime.Modules
{
    public sealed class CoreModule : RuntimeModule
    {
        public override string FullyQualifiedName => "core";

        [RuntimeModuleFunction("gc.collect", ReturnType = DynamicValueType.Nil)]
        private static DynamicValue GcCollect(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNone();
            GC.Collect(
                GC.MaxGeneration, 
                GCCollectionMode.Aggressive, 
                false, 
                true
            );

            return Nil;
        }

        [RuntimeModuleFunction("strace", ReturnType = DynamicValueType.Array)]
        private static DynamicValue StackTrace(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectAtMost(1)
                .OptionalBooleanAt(0, defaultValue: false, out var skipNativeFrames);
            
            var callStack = fiber.CallStack.ToArray(skipNativeFrames);
            var array = new Array(callStack.Length);
            
            for (var i = 0; i < callStack.Length; i++)
            {
                if (callStack[i] is ScriptStackFrame ssf)
                {
                    array[i] = new Table
                    {
                        { "is_script", true },
                        { "fn_name", ssf.Chunk.Name ?? "<unknown>" },
                        { "ip", ssf.IP },
                        { "line", ssf.Chunk.HasDebugInfo ? ssf.Chunk.DebugDatabase.GetLineForIP((int)ssf.PreviousOpCodeIP) : Nil },
                        { "locals", ssf.Locals?.ToTable() ?? Nil },
                        { "args", ssf.Arguments.ToTable() },
                        { "xargs", ssf.ExtraArguments.DeepCopy() },
                        { "def_on_line", ssf.Chunk.DebugDatabase.DefinedOnLine > 0 ? ssf.Chunk.DebugDatabase.DefinedOnLine : Nil },
                        { "def_in_file", !string.IsNullOrEmpty(ssf.Chunk.DebugDatabase.DefinedInFile) ? ssf.Chunk.DebugDatabase.DefinedInFile : Nil }
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

        [RuntimeModuleFunction("strace_s", ReturnType = DynamicValueType.String)]
        private static DynamicValue StackTraceString(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectAtMost(1)
                .OptionalBooleanAt(0, defaultValue: false, out var skipNativeFrames);

            return fiber.StackTrace(skipNativeFrames);
        }
    }
}
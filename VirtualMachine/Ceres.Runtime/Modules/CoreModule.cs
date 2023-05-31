using System.Text;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.Runtime.Modules
{
    public sealed class CoreModule : RuntimeModule
    {
        public override string FullyQualifiedName => "core";
        
        [RuntimeModuleFunction("print", ReturnType = DynamicValueType.Number)]
        private static DynamicValue Print(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1);
            
            var str = string.Join(
                "\t",
                args.Select(x => x.ConvertToString().String)
            );

            Console.Write(str);
            return str.Length;
        }
        
        [RuntimeModuleFunction("println", ReturnType = DynamicValueType.Number)]
        private static DynamicValue PrintLine(Fiber _, params DynamicValue[] args)
        {
            var str = string.Join(
                "\t",
                args.Select(x => x.ConvertToString().String)
            );

            Console.Write(str);
            Console.Write(Environment.NewLine);
            
            return str.Length + Environment.NewLine.Length;
        }

        [RuntimeModuleFunction("strace", ReturnType = DynamicValueType.Table)]
        private static DynamicValue StackTrace(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectAtMost(1)
                .OptionalBooleanAt(0, defaultValue: false, out var skipNativeFrames);
            
            var ret = new Table();
            StackFrame[] callStack;

            if (skipNativeFrames)
            {
                callStack = fiber.CallStack.Where(x => x is ScriptStackFrame).ToArray();
            }
            else
            {
                callStack = fiber.CallStack.ToArray();
            }
            
            for (var i = 0; i < callStack.Length; i++)
            {
                if (callStack[i] is ScriptStackFrame ssf)
                {
                    ret[i] = new Table
                    {
                        { "is_script", true },
                        { "fn_name", ssf.Chunk.Name ?? "<unknown>" },
                        { "ip", ssf.IP },
                        { "line", ssf.Chunk.HasDebugInfo ? ssf.Chunk.DebugDatabase.GetLineForIP((int)ssf.PreviousOpCodeIP) : Nil },
                        { "locals", ssf.Locals?.ToTable() ?? Nil },
                        { "args", ssf.Arguments.ToTable() },
                        { "xargs", ssf.ExtraArguments.DeepCopy() },
                        { "def_on_line", ssf.Chunk.HasDebugInfo ? ssf.Chunk.DebugDatabase.DefinedOnLine : Nil },
                    };
                }
                else if (callStack[i] is NativeStackFrame nsf && !skipNativeFrames)
                {
                    ret[i] = new Table
                    {
                        { "is_script", false },
                        { "fn_name", nsf.NativeFunction.Method.Name },
                        { "decl_type", nsf.NativeFunction.Method.DeclaringType!.FullName! },
                    };
                }
            }
            
            return ret;
        }

        [RuntimeModuleFunction("strace_s", ReturnType = DynamicValueType.Table)]
        private static DynamicValue StackTraceString(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectAtMost(1)
                .OptionalBooleanAt(0, defaultValue: false, out var skipNativeFrames);

            return fiber.StackTrace(skipNativeFrames);
        }
    }
}
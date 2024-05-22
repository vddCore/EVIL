using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;
using Array = EVIL.Ceres.ExecutionEngine.Collections.Array;

namespace EVIL.Ceres.Runtime.Modules
{
    public partial class DebugModule : RuntimeModule
    {
        public override string FullyQualifiedName => "debug";

        [RuntimeModuleFunction("strace")]
        [EvilDocFunction(
            "Gets the current stack trace in the form of raw data.",
            Returns = "Array containing Table values with stack frame information available at the time of invocation.",
            ReturnType = DynamicValueType.Array
        )]
        [EvilDocArgument(
            "skip_native_frames",
            "Set to `true` to skip CLR (native) frames, `false` to include them in the output.",
            DynamicValueType.Boolean,
            DefaultValue = "false"
        )]
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
                        { "line", ssf.Chunk.HasDebugInfo ? ssf.Chunk.DebugDatabase.GetLineForIP((int)ssf.PreviousOpCodeIP) : DynamicValue.Nil },
                        { "locals", ssf.Locals?.ToTable() ?? DynamicValue.Nil },
                        { "args", ssf.Arguments.ToTable() },
                        { "xargs", new Array(ssf.ExtraArguments) },
                        { "has_self", ssf.Chunk.IsSelfAware },
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
            ReturnType = DynamicValueType.Boolean
        )]
        [EvilDocArgument(
            "skip_native_frames",
            "Set to `true` to CLR (native) frames, `false` to include them in the output.",
            DynamicValueType.Boolean,
            DefaultValue = "false"
        )]
        private static DynamicValue StackTraceString(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectAtMost(1)
                .OptionalBooleanAt(0, defaultValue: false, out var skipNativeFrames);

            return fiber.StackTrace(skipNativeFrames);
        }

        [RuntimeModuleFunction("assert")]
        [EvilDocFunction(
            "Throws if the provided condition does not evaluate to a `true`.",
            ReturnType = DynamicValueType.Nil
        )]
        [EvilDocArgument(
            "expr",
            "Expression to be evaluated and checked for truth.",
            DynamicValueType.Boolean,
            CanBeNil = false
        )]
        [EvilDocArgument(
            "fail_msg",
            "Message to be set for the thrown Error if assertion fails.",
            DynamicValueType.String,
            CanBeNil = true,
            DefaultValue = "Assertion failed."
        )]
        private static DynamicValue Assert(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1)
                .ExpectBooleanAt(0, out var expr)
                .OptionalStringAt(1, "Assertion failed.", out var failMsg);

            if (!expr)
            {
                return fiber.ThrowFromNative(new Error(failMsg));
            }
            
            return DynamicValue.Nil;
        }
    }
}
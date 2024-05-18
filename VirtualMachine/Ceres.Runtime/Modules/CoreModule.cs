using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.Runtime.Modules
{
    public sealed class CoreModule : RuntimeModule
    {
        public override string FullyQualifiedName => "core";

        [RuntimeModuleFunction("fail")]
        [EvilDocFunction(
            "Terminates VM execution with a user-requested runtime failure. This function does not return."
        )]
        [EvilDocArgument(
            "message",
            "The message that should be displayed for the failure.",
            DynamicValueType.String,
            DefaultValue = "no details provided"
        )]
        private static DynamicValue Fail(Fiber fiber, params DynamicValue[] args)
        {
            args.OptionalStringAt(0, "no details provided", out var message);
            throw new UserFailException(message, fiber, args);
        }
    }
}
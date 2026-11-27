namespace EVIL.VM.Runtime.Modules;

using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine.Concurrency;
using EVIL.VM.ExecutionEngine.TypeSystem;
using EVIL.VM.Runtime.Extensions;

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
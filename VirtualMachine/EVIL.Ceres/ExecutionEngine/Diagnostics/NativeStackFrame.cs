namespace EVIL.Ceres.ExecutionEngine.Diagnostics;

public sealed record NativeStackFrame(NativeFunction NativeFunction)
    : StackFrame
{
    public bool HasThrown { get; internal set; }

    public override string ToString()
        => $"clr!{NativeFunction.Method.DeclaringType!.FullName}::{NativeFunction.Method.Name}";
}
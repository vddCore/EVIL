namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed record NativeStackFrame : StackFrame
    {
        public NativeFunction NativeFunction { get; }

        public NativeStackFrame(NativeFunction nativeFunction)
        {
            NativeFunction = nativeFunction;
        }
        
        public override void Dispose()
        {
        }

        public override string ToString()
            => $"clr!{NativeFunction.Method.DeclaringType!.FullName}::{NativeFunction.Method.Name}";
    }
}
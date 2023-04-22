namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed class NativeStackFrame : StackFrame
    {
        public NativeFunction NativeFunction { get; }

        public NativeStackFrame(NativeFunction nativeFunction)
        {
            NativeFunction = nativeFunction;
        }
        
        public override void Dispose()
        {
        }
    }
}
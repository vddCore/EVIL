namespace EVIL.VM.ExecutionEngine.Diagnostics;

public abstract record StackFrame
{
    public T As<T>() where T : StackFrame 
        => (T)this;
}
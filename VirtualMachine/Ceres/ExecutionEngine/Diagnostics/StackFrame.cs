using System;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public abstract record StackFrame : IDisposable
    {
        public T As<T>() where T : StackFrame 
            => (T)this;

        public abstract void Dispose();
    }
}
namespace EVIL.VM.ExecutionEngine.Marshaling.Adapters;

using System;
using EVIL.VM.ExecutionEngine.TypeSystem;

public interface IContextualValueAdapter<T>
{
    DynamicValue ToDynamicValue(T value, MarshalingContext context)
        => throw new NotSupportedException("This adapter does not support CLR-to-EVIL conversion.");

    T FromDynamicValue(DynamicValue value, MarshalingContext context)
        => throw new NotSupportedException("This adapter does not support EVIL-to-CLR conversion.");
}
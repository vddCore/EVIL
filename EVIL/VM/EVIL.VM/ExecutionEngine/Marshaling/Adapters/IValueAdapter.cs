namespace EVIL.VM.ExecutionEngine.Marshaling.Adapters;

using System;
using EVIL.VM.ExecutionEngine.TypeSystem;

public interface IValueAdapter<T>
{
    DynamicValue ToDynamicValue(T value)
        => throw new NotSupportedException("This adapter does not support CLR-to-EVIL conversion.");

    T FromDynamicValue(DynamicValue value)
        => throw new NotSupportedException("This adapter does not support EVIL-to-CLR conversion.");
}
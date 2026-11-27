namespace EVIL.VM.ExecutionEngine.Marshaling.Adapters;

using System;
using EVIL.VM.ExecutionEngine.TypeSystem;

public sealed class DelegatingAdapter<T>(
    Func<T, DynamicValue> toDynamicValue,
    Func<DynamicValue, T> fromDynamicValue)
    : IValueAdapter<T>
{
    public DynamicValue ToDynamicValue(T value)
        => toDynamicValue(value);
    
    public T FromDynamicValue(DynamicValue value)
        => fromDynamicValue(value);
}
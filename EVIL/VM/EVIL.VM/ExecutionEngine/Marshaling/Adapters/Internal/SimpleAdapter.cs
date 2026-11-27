namespace EVIL.VM.ExecutionEngine.Marshaling.Adapters.Internal;

using System;
using EVIL.VM.ExecutionEngine.TypeSystem;

internal abstract class SimpleAdapter
{
    public abstract Type NativeType { get; }
        
    public abstract DynamicValue ToDynamicValue(
        object value,
        MarshalingContext context
    );
        
    public abstract object? FromDynamicValue(
        DynamicValue value,
        MarshalingContext context
    );
}

internal sealed class SimpleAdapter<T>(IValueAdapter<T> adapter) 
    : SimpleAdapter
{
    public override Type NativeType => typeof(T);
    
    public override DynamicValue ToDynamicValue(object value, MarshalingContext context)
        => adapter.ToDynamicValue((T)value);
    
    public override object? FromDynamicValue(DynamicValue value, MarshalingContext context)
        => adapter.FromDynamicValue(value);
}

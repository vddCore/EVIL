namespace EVIL.VM.ExecutionEngine.Marshaling.Adapters.Internal;

using System;
using EVIL.VM.ExecutionEngine.TypeSystem;

internal sealed class ContextualAdapter<T>(IContextualValueAdapter<T> adapter) 
    : SimpleAdapter
{
    public override Type NativeType 
        => typeof(T);
    
    public override DynamicValue ToDynamicValue(
        object value,
        MarshalingContext context)
        => adapter.ToDynamicValue((T)value, context);
    
    public override object? FromDynamicValue(
        DynamicValue value,
        MarshalingContext context)
        => adapter.FromDynamicValue(value, context);
}

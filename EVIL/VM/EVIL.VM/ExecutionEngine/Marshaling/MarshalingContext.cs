namespace EVIL.VM.ExecutionEngine.Marshaling;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine.TypeSystem;

public sealed class MarshalingContext
{
    private readonly MarshalingRegistry _registry;
    
    private readonly Dictionary<object, DynamicValue> _dynamicValues 
        = new(ReferenceComparer.Instance);
    
    private readonly Dictionary<object, List<object>> _nativeValues
        = new(ReferenceComparer.Instance);
    
    private readonly HashSet<object> _activeClrReferences 
        = new(ReferenceComparer.Instance);
    
    private readonly Dictionary<object, HashSet<Type>> _activeNativeReferences 
        = new(ReferenceComparer.Instance);

    internal MarshalingContext(MarshalingRegistry registry)
    {
        _registry = registry;
    }

    public DynamicValue ToDynamicValue(object? value)
        => _registry.ToDynamicValue(value, this);

    public T ToNative<T>(DynamicValue value)
        => (T)_registry.ToNative(value, typeof(T), this)!;

    public object? ToNative(DynamicValue value, Type targetType)
        => _registry.ToNative(value, targetType, this);

    public void RegisterDynamicValue(object source, DynamicValue converted)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source.GetType().IsValueType)
        {
            return;
        }

        if (_dynamicValues.TryGetValue(source, out var existing))
        {
            if (existing != converted)
            {
                throw new InvalidOperationException(
                    "A different EVIL value is already registered for this CLR object."
                );
            }

            return;
        }

        _dynamicValues.Add(source, converted);
    }

    public void RegisterNativeObject(DynamicValue source, object converted)
    {
        ArgumentNullException.ThrowIfNull(converted);

        var sourceReference = GetReference(source);
        if (sourceReference == null)
        {
            throw new ArgumentException(
                "Only EVIL tables, arrays, and native objects can be registered for identity reuse.", 
                nameof(source)
            );
        }

        if (!_nativeValues.TryGetValue(sourceReference, out var convertedValues))
        {
            convertedValues = [];
            _nativeValues.Add(sourceReference, convertedValues);
        }

        if (!convertedValues.Exists(value => ReferenceEquals(value, converted)))
        {
            convertedValues.Add(converted);
        }
    }

    internal bool TryGetDynamicValue(object source, out DynamicValue value)
    {
        if (source.GetType().IsValueType)
        {
            value = default(DynamicValue);
            return false;
        }

        return _dynamicValues.TryGetValue(source, out value);
    }

    internal bool TryEnterClrReference(object source)
        => source.GetType().IsValueType || _activeClrReferences.Add(source);

    internal void ExitClrReference(object source)
    {
        if (!source.GetType().IsValueType)
        {
            _activeClrReferences.Remove(source);
        }
    }

    internal bool TryGetNativeObject(DynamicValue source, Type targetType, out object? value)
    {
        var sourceReference = GetReference(source);
        if (sourceReference != null && _nativeValues.TryGetValue(sourceReference, out var convertedValues))
        {
            foreach (var converted in convertedValues)
            {
                if (targetType.IsInstanceOfType(converted))
                {
                    value = converted;
                    return true;
                }
            }
        }

        value = null;
        return false;
    }

    internal bool TryEnterNativeReference(DynamicValue source, Type targetType)
    {
        var sourceReference = GetReference(source);
        if (sourceReference == null)
        {
            return true;
        }

        if (!_activeNativeReferences.TryGetValue(sourceReference, out var targetTypes))
        {
            targetTypes = [];
            _activeNativeReferences.Add(sourceReference, targetTypes);
        }

        return targetTypes.Add(targetType);
    }

    internal void ExitNativeReference(DynamicValue source, Type targetType)
    {
        var sourceReference = GetReference(source);
        if (sourceReference != null
            && _activeNativeReferences.TryGetValue(sourceReference, out var targetTypes))
        {
            targetTypes.Remove(targetType);
            if (targetTypes.Count == 0)
            {
                _activeNativeReferences.Remove(sourceReference);
            }
        }
    }
    
    internal bool HasReference(DynamicValue value)
        => GetReference(value) != null;

    private static object? GetReference(DynamicValue value)
    {
        return value.Type switch
        {
            DynamicValueType.Table => value.Table,
            DynamicValueType.Array => value.Array,
            DynamicValueType.NativeObject => value.NativeObject,
            _ => null
        };
    }
}
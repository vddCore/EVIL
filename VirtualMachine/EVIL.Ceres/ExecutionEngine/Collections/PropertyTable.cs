namespace EVIL.Ceres.ExecutionEngine.Collections;

using System;
using System.Collections.Generic;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

public class PropertyTable : Table
{
    public delegate DynamicValue TableGetter(DynamicValue key);

    public delegate (DynamicValue Key, DynamicValue Value) TableSetter(DynamicValue key, DynamicValue value);

    private readonly Dictionary<DynamicValue, TableGetter> _getters = new();
    private readonly Dictionary<DynamicValue, TableSetter> _setters = new();

    public PropertyTable()
    {
    }

    public PropertyTable(PropertyTable collection)
        : base(collection)
    {
        foreach (var (key, value) in collection._getters)
        {
            _getters[key] = value;
        }

        foreach (var (key, value) in collection._setters)
        {
            _setters[key] = value;
        }
    }

    public void Add(DynamicValue key, TableSetter setter)
        => AddSetter(key, setter);

    public void Add(DynamicValue key, TableGetter getter)
        => AddGetter(key, getter);

    public virtual void AddGetter(DynamicValue key, TableGetter getter)
    {
        if (!_getters.TryAdd(key, getter))
        {
            throw new InvalidOperationException(
                $"Attempt to register a duplicate getter for '{key.ToString()}'"
            );
        }
    }

    public virtual bool RemoveGetter(DynamicValue key)
        => _getters.Remove(key);

    public virtual void AddSetter(DynamicValue key, TableSetter setter)
    {
        if (!_setters.TryAdd(key, setter))
        {
            throw new InvalidOperationException(
                $"Attempt to register a duplicate setter for '{key.ToString()}'"
            );
        }
    }

    public virtual bool RemoveSetter(DynamicValue key)
        => _setters.Remove(key);

    protected override bool OnTryGetValue(DynamicValue key, out DynamicValue value)
    {
        if (_getters.Count > 0 && _getters.TryGetValue(key, out var getter))
        {
            value = getter(key);
            return true;
        }

        return base.OnTryGetValue(key, out value);
    }

    protected override DynamicValue OnIndex(DynamicValue key)
    {
        return OnTryGetValue(key, out var val) 
            ? val 
            : DynamicValue.Nil;
    }

    protected override (DynamicValue Key, DynamicValue Value) OnBeforeSet(DynamicValue key, DynamicValue value)
    {
        return _setters.TryGetValue(key, out var setter) 
            ? setter(key, value)
            : base.OnBeforeSet(key, value);
    }

    public virtual bool ContainsGetter(DynamicValue key)
        => _getters.ContainsKey(key);
        
    public virtual bool ContainsSetter(DynamicValue key)
        => _setters.ContainsKey(key);

    public virtual bool ContainsValue(DynamicValue key)
        => base.OnContains(key);

    protected override bool OnContains(DynamicValue key)
    {
        return ContainsGetter(key)
               || ContainsSetter(key)
               || ContainsValue(key);
    }
}
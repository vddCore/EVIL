namespace EVIL.Ceres.ExecutionEngine.Collections;

using System;
using System.Collections.Generic;
using EVIL.Ceres.ExecutionEngine.TypeSystem;

public sealed class TableShape
{
    public static readonly TableShape Empty = new(null, null, -1, 0);

    private readonly Dictionary<string, TableShape> _transitions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _propertySlots;
    private readonly string? _propertyName;
    private readonly int _slot;
    private readonly TableShape? _parent;

    public int PropertyCount { get; }
    public DynamicValue[] PropertyKeys { get; }
    
    public IReadOnlyDictionary<string, int> PropertySlots => _propertySlots;

    private TableShape(TableShape? parent, string? propertyName, int slot, int propertyCount)
    {
        _parent = parent;
        _propertyName = propertyName;
        _slot = slot;
        PropertyCount = propertyCount;

        if (parent == null)
        {
            _propertySlots = new Dictionary<string, int>(
                StringComparer.Ordinal
            );
            
            PropertyKeys = [];
        }
        else
        {
            _propertySlots = new Dictionary<string, int>(
                parent._propertySlots, 
                StringComparer.Ordinal)
            {
                [propertyName!] = slot
            };

            PropertyKeys = new DynamicValue[propertyCount];
            parent.PropertyKeys.CopyTo(PropertyKeys, 0);
            
            PropertyKeys[slot] = new DynamicValue(propertyName!);
        }
    }

    public TableShape Transition(string propertyName, out int slot)
    {
        lock (_transitions)
        {
            if (_transitions.TryGetValue(propertyName, out var nextShape))
            {
                slot = nextShape._slot;
                return nextShape;
            }
            slot = PropertyCount;
            
            nextShape = new TableShape(this, propertyName, slot, PropertyCount + 1);
            _transitions[propertyName] = nextShape;
            
            return nextShape;
        }
    }

    public bool TryGetSlot(string propertyName, out int slot)
    {
        return _propertySlots.TryGetValue(propertyName, out slot);
    }
}

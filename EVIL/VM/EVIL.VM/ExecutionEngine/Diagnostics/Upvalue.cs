namespace EVIL.VirtualMachine.ExecutionEngine.Diagnostics;

using System.Runtime.CompilerServices;
using EVIL.VirtualMachine.ExecutionEngine.TypeSystem;

public sealed class Upvalue
{
    public DynamicValue Value;
    private DynamicValue[]? _liveStorage;
    private readonly int _liveSlot;

    public bool IsClosed => _liveStorage == null;

    public Upvalue(DynamicValue initialValue)
    {
        Value = initialValue;
    }

    public Upvalue(DynamicValue initialValue, DynamicValue[]? liveStorage, int liveSlot)
    {
        Value = initialValue;
        _liveStorage = liveStorage;
        _liveSlot = liveSlot;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DynamicValue Get() => _liveStorage != null ? _liveStorage[_liveSlot] : Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(DynamicValue value)
    {
        Value = value;
        if (_liveStorage != null)
        {
            _liveStorage[_liveSlot] = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Close()
    {
        if (_liveStorage != null)
        {
            Value = _liveStorage[_liveSlot];
            _liveStorage = null;
        }
    }
}

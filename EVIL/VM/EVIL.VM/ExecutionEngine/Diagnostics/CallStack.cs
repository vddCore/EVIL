namespace EVIL.VirtualMachine.ExecutionEngine.Diagnostics;

using System;
using System.Linq;
using System.Runtime.CompilerServices;

public class CallStack
{
    public const int MaxFrames = 16384;
    internal const int InitialCapacity = 64;

    private int _currentPointer = -1;
    private StackFrame?[] _frames = new StackFrame[InitialCapacity];

    public StackFrame this[int index] => _frames[_currentPointer - index]!;
    public int Count => _currentPointer + 1;
    public int Capacity => _frames.Length;

    internal void TrimExcess()
    {
        var targetCapacity = Math.Max(InitialCapacity, Count);
        if (targetCapacity < _frames.Length)
        {
            Array.Resize(ref _frames, targetCapacity);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StackFrame Peek()
    {
        return _currentPointer < 0 
            ? throw new InvalidOperationException("Stack empty") 
            : _frames[_currentPointer]!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(StackFrame stackFrame)
    {
        if (_currentPointer + 1 >= MaxFrames)
        {
            throw new VirtualMachineException("Call stack overflow.");
        }

        if (_currentPointer + 1 >= _frames.Length)
        {
            Array.Resize(
                ref _frames, 
                Math.Min(
                    _frames.Length * 2, 
                    MaxFrames
                )
            );
        }

        _frames[++_currentPointer] = stackFrame;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StackFrame Pop()
    {
        if (_currentPointer < 0)
        {
            throw new InvalidOperationException("Stack empty.");
        }

        var ret = _frames[_currentPointer]!;
        _frames[_currentPointer--] = null;

        return ret;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Array.Clear(_frames);
        _currentPointer = -1;
    }

    public StackFrame[] ToArray(bool skipNativeFrames = false)
    {
        if (skipNativeFrames)
        {
            return _frames.Take(_currentPointer + 1)
                .Where(x => x is ScriptStackFrame)
                .Reverse()
                .ToArray()!;
        }
        else
        {
            return _frames.Take(_currentPointer + 1)
                .Reverse()
                .ToArray()!;
        }
    }
}
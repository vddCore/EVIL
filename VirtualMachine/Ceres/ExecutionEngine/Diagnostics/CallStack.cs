using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public class CallStack
    {
        public const int MaxFrames = 16384;

        private int _currentPointer = -1;
        private StackFrame?[] _frames = new StackFrame[MaxFrames];

        public StackFrame this[int index] => _frames[_currentPointer - index]!;
        public int Count => _currentPointer + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackFrame Peek()
        {
            if (_currentPointer < 0)
            {
                throw new InvalidOperationException("Stack empty");
            }

            return _frames[_currentPointer]!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(StackFrame stackFrame)
        {
            if (_currentPointer + 1 >= MaxFrames)
            {
                throw new VirtualMachineException("Call stack overflow.");
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
        public void DisposeAllAndClear()
        {
            for (var i = 0; i < _frames.Length; i++)
            {
                if (_frames[i] == null)
                    break;

                _frames[i]!.Dispose();
            }

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
}
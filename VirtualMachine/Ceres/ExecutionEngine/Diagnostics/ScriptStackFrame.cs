using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed class ScriptStackFrame : StackFrame
    {
        private readonly BinaryReader _chunkReader;
        private Table? _extraArguments;
        
        private readonly Stack<IEnumerator<KeyValuePair<DynamicValue, DynamicValue>>> _tableEnumerators = new();

        public Fiber Fiber { get; }
        public Chunk Chunk { get; }

        public DynamicValue[] Arguments { get; }
        public Table ExtraArguments => _extraArguments ??= GetExtraArgumentsTable();

        public DynamicValue[]? Locals { get; }
        public DynamicValue[]? Closures { get; }
        
        public long PreviousOpCodeIP { get; private set; }
        public long IP => _chunkReader.BaseStream.Position;

        public IEnumerator<KeyValuePair<DynamicValue, DynamicValue>>? CurrentEnumerator
        {
            get
            {
                if (_tableEnumerators.TryPeek(out var enumerator))
                    return enumerator;

                return null;
            }
        }

        internal ScriptStackFrame(Fiber fiber, Chunk chunk, DynamicValue[] args)
        {
            Fiber = fiber;
            Chunk = chunk;

            if (chunk.ParameterCount > args.Length)
            {
                Arguments = new DynamicValue[chunk.ParameterCount];
            }
            else
            {
                Arguments = new DynamicValue[args.Length];
            }

            for (var i = 0; i < Arguments.Length; i++)
            {
                if (Chunk.ParameterInitializers.TryGetValue(i, out var initializer))
                {
                    Arguments[i] = initializer;
                }

                if (i < args.Length)
                {
                    Arguments[i] = args[i];
                }
            }

            if (chunk.LocalCount > 0)
            {
                Locals = new DynamicValue[chunk.LocalCount];
                Array.Fill(Locals, DynamicValue.Nil);
            }

            if (chunk.ClosureCount > 0)
            {
                Closures = new DynamicValue[chunk.ClosureCount];
                Array.Fill(Closures, DynamicValue.Nil);
            }

            _chunkReader = Chunk.SpawnCodeReader();
        }

        internal DynamicValue[] GetExtraArguments()
        {
            var size = Arguments.Length - Chunk.ParameterCount;
            if (size < 0) size = 0;
            var ret = new DynamicValue[size];

            for (var i = Chunk.ParameterCount; i < Arguments.Length; i++)
            {
                ret[i - Chunk.ParameterCount] = Arguments[i];
            }

            return ret;
        }

        internal Table GetExtraArgumentsTable()
        {
            var ret = new Table();

            for (var i = Chunk.ParameterCount; i < Arguments.Length; i++)
            {
                ret[i - Chunk.ParameterCount] = Arguments[i];
            }

            ret.Freeze();
            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PushEnumerator(Table table)
            => _tableEnumerators.Push(table.GetEnumerator());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PopEnumerator()
            => _tableEnumerators.Pop();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Return()
            => _chunkReader.BaseStream.Seek(0, SeekOrigin.End);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Advance() 
            => _chunkReader.BaseStream.Position++;

        internal OpCode PeekOpCode()
        {            
            var opCode = FetchOpCode();

            _chunkReader.BaseStream.Position--;
            return opCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal OpCode FetchOpCode()
        {
            PreviousOpCodeIP = IP;
            return (OpCode)_chunkReader.ReadByte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal byte FetchByte()
            => _chunkReader.ReadByte();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal double FetchDouble()
            => _chunkReader.ReadDouble();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int FetchInt32()
            => _chunkReader.ReadInt32();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal long FetchInt64()
            => _chunkReader.ReadInt64();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void JumpAbsolute(long address)
            => _chunkReader.BaseStream.Seek(address, SeekOrigin.Begin);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void JumpRelative(long offset)
            => _chunkReader.BaseStream.Seek(offset, SeekOrigin.Current);

        public override void Dispose()
        {
            _chunkReader.Dispose();
        }
    }
}
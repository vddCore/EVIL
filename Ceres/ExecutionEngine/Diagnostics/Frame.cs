using System;
using System.Collections.Generic;
using System.IO;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed class Frame : IDisposable
    {
        private readonly BinaryReader _chunkReader;
        
        public Fiber Fiber { get; }
        public Chunk Chunk { get; }

        public DynamicValue[]? Arguments { get; }
        public DynamicValue[]? Locals { get; }

        public long IP => _chunkReader.BaseStream.Position;
        
        internal Frame(Fiber fiber, Chunk chunk, DynamicValue[] args)
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
                if (i < args.Length)
                {
                    Arguments[i] = args[i];
                }
                else
                {
                    Arguments[i] = DynamicValue.Nil;
                }
            }

            if (chunk.LocalCount > 0)
            {
                Locals = new DynamicValue[chunk.LocalCount];
                Array.Fill(Locals, DynamicValue.Nil);
            }
            
            _chunkReader = Chunk.SpawnCodeReader();
        }

        internal void Return() 
            => _chunkReader.BaseStream.Seek(0, SeekOrigin.End);

        internal void Advance()
            => _chunkReader.BaseStream.Position++;

        internal OpCode PeekOpCode()
        {
            var opCode = FetchOpCode();
            
            _chunkReader.BaseStream.Position--;
            return opCode;
        }
        
        internal OpCode FetchOpCode()
            => (OpCode)_chunkReader.ReadByte();

        internal byte FetchByte()
            => _chunkReader.ReadByte();

        internal double FetchDouble()
            => _chunkReader.ReadDouble();

        internal int FetchInt32()
            => _chunkReader.ReadInt32();
        
        internal long FetchInt64()
            => _chunkReader.ReadInt64();

        internal void JumpAbsolute(long address) 
            => _chunkReader.BaseStream.Seek(address, SeekOrigin.Begin);

        internal void JumpRelative(long offset)
            => _chunkReader.BaseStream.Seek(offset, SeekOrigin.Current);

        public void Dispose()
        {
            _chunkReader.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ceres.ExecutionEngine.Diagnostics;

namespace Ceres.ExecutionEngine
{
    public sealed class CodeGenerator : IDisposable
    {
        private readonly MemoryStream _code;
        private readonly BinaryWriter _writer;
        private readonly List<int> _labels;

        public int IP
        {
            get => (int)_code.Position;
            set => _code.Seek(value, SeekOrigin.Begin);
        }
        
        public IReadOnlyList<int> Labels => _labels;
        
        internal CodeGenerator(MemoryStream code, List<int> labels)
        {
            _code = code;
            _labels = labels;
            
            _writer = new BinaryWriter(_code, Encoding.UTF8, true);
        }

        public OpCode PeekOpCode()
        {
            if (_code.Length == 0)
            {
                throw new InvalidOperationException("Chunk contains no instructions.");
            }
            
            _code.Seek(-1, SeekOrigin.Current);
            var opCode = (OpCode)_code.ReadByte();
            return opCode;
        }

        public bool TryPeekOpCode(out OpCode? opCode)
        {
            opCode = null;

            try
            {
                opCode = PeekOpCode();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Emit(OpCode value)
            => _writer.Write((byte)value);

        public void Emit(OpCode value, double operand)
        {
            Emit(value);
            Emit(operand);
        }

        public void Emit(OpCode value, long operand)
        {
            Emit(value);
            Emit(operand);
        }
        
        public void Emit(OpCode value, int operand)
        {
            Emit(value);
            Emit(operand);
        }
        
        public void Emit(double value) 
            => _writer.Write(value);

        public void Emit(long value) 
            => _writer.Write(value);

        public void Emit(int value)
            => _writer.Write(value);

        public void Emit(bool value)
            => _writer.Write(value);

        public long Label(int offset = 0)
        {
            _labels.Add(IP + offset);
            return _labels[_labels.Count - 1];
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
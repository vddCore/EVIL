using System.Collections.Generic;

namespace EVIL.Intermediate
{
    public class CodeGenerator
    {
        private List<byte> _instructions;

        internal CodeGenerator(Chunk chunk)
        {
            _instructions = chunk.Instructions;
        }

        public void Emit(OpCode opCode, double operand)
        {
            Emit(opCode);
            Emit(operand);
        }

        public void Emit(OpCode opCode, int operand)
        {
            Emit(opCode);
            Emit(operand);
        }

        public void Emit(OpCode opCode)
            => _instructions.Add((byte)opCode);

        private void Emit(int primitive)
        {
            unsafe
            {
                var data = (byte*)&primitive;
                
                _instructions.Add(data[0]);
                _instructions.Add(data[1]);
                _instructions.Add(data[2]);
                _instructions.Add(data[3]);
            }
        }

        private void Emit(double primitive)
        {
            unsafe
            {
                var data = (byte*)&primitive;
                
                _instructions.Add(data[0]);
                _instructions.Add(data[1]);
                _instructions.Add(data[2]);
                _instructions.Add(data[3]);
                _instructions.Add(data[4]);
                _instructions.Add(data[5]);
                _instructions.Add(data[6]);
                _instructions.Add(data[7]);
            }
        }
    }
}
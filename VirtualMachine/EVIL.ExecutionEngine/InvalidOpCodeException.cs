using System;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.ExecutionEngine
{
    public class InvalidOpCodeException : Exception
    {
        public OpCode OpCode { get; }

        public InvalidOpCodeException(OpCode opCode)
            : base("Invalid op-code.")
        {
            OpCode = opCode;
        }
    }
}
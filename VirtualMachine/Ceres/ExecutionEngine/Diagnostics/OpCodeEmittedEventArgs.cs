using System;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed class OpCodeEmittedEventArgs : EventArgs
    {
        public int IP { get; }
        public OpCode OpCode { get; }

        internal OpCodeEmittedEventArgs(int ip, OpCode opCode)
        {
            IP = ip;
            OpCode = opCode;
        }
    }
}
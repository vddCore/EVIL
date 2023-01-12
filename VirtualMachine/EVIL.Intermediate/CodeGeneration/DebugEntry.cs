using System;

namespace EVIL.Intermediate.CodeGeneration
{
    public struct DebugEntry
    {
        public int Line { get; }
        public int Column { get; }
        public int IP { get; }

        public DebugEntry(int line, int column, int ip)
        {
            Line = line;
            Column = column;
            IP = ip;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Line, Column, IP);
        }
    }
}
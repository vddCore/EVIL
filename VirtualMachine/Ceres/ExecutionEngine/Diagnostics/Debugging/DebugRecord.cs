using System;

namespace Ceres.ExecutionEngine.Diagnostics.Debugging
{
    public struct DebugRecord : IEquatable<DebugRecord>
    {
        public int Line { get; } = -1;
        public int IP { get; } = -1;

        public static readonly DebugRecord Empty = new(-1, -1);

        public DebugRecord(int line, int ip)
        {
            Line = line;
            IP = ip;
        }

        public bool Equals(DebugRecord other) 
            => Line == other.Line 
            && IP == other.IP;

        public override bool Equals(object? obj) 
            => obj is DebugRecord other 
            && Equals(other);

        public override int GetHashCode() 
            => HashCode.Combine(Line, IP);

        public static bool operator ==(DebugRecord left, DebugRecord right) 
            => left.Equals(right);

        public static bool operator !=(DebugRecord left, DebugRecord right) 
            => !left.Equals(right);
    }
}
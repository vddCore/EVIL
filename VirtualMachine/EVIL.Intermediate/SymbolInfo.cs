using System;

namespace EVIL.Intermediate
{
    public struct SymbolInfo
    {
        public enum SymbolType
        {
            Undefined = -1,
            Global,
            Local,
            Parameter
        }

        public static readonly SymbolInfo Undefined = new(-1, SymbolType.Undefined);
        
        public int Id { get; }
        public SymbolType Type { get; }

        public SymbolInfo(int id, SymbolType type)
        {
            Id = id;
            Type = type;
        }

        public bool Equals(SymbolInfo other)
            => (Id, Type) == (other.Id, other.Type);

        public override bool Equals(object obj) 
            => obj is SymbolInfo other && Equals(other);

        public override int GetHashCode() 
            => HashCode.Combine(Id, (int)Type);

        public static bool operator ==(SymbolInfo left, SymbolInfo right) 
            => left.Equals(right);

        public static bool operator !=(SymbolInfo left, SymbolInfo right) 
            => !left.Equals(right);
    }
}
namespace EVIL.Intermediate.CodeGeneration
{
    public struct ExternInfo
    {
        public enum ExternType : byte
        {
            Local,
            Parameter,
            Extern
        }
        
        public string Name { get; }
        public string OwnerChunkName { get; }
        public int SymbolId { get; }
        public ExternType Type { get; }

        public ExternInfo(string name, string ownerChunkName, int symbolId, ExternType type)
        {
            Name = name;
            OwnerChunkName = ownerChunkName;
            SymbolId = symbolId;
            Type = type;
        }
    }
}
namespace EVIL.Intermediate
{
    public struct ExternInfo
    {
        public string Name { get; }
        public string OwnerChunkName { get; }
        public int SymbolId { get; }
        public bool IsParameter { get; }

        public ExternInfo(string name, string ownerChunkName, int symbolId, bool isParameter)
        {
            Name = name;
            OwnerChunkName = ownerChunkName;
            SymbolId = symbolId;
            IsParameter = isParameter;
        }
    }
}
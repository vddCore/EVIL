namespace EVIL.Intermediate
{
    public struct ExternInfo
    {
        public string Name { get; }
        public string OwnerChunkName { get; }
        public int OwnerLocalId { get; }
        public bool IsParam { get; }

        public ExternInfo(string name, string ownerChunkName, int ownerLocalId, bool isParam)
        {
            Name = name;
            OwnerChunkName = ownerChunkName;
            OwnerLocalId = ownerLocalId;
            IsParam = isParam;
        }
    }
}
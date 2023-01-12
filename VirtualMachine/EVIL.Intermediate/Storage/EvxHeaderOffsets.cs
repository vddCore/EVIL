namespace EVIL.Intermediate.Storage
{
    public static class EvxHeaderOffsets
    {
        public const long MagicNumber = 0x0000; // 3 bytes
        public const long FormatVersion = 0x0003; // 1 byte
        public const long Checksum = 0x0004; // 8 bytes
        public const long TimeStamp = 0x000C; // 8 bytes
        public const long LinkerID = 0x0014; // 4 bytes
        public const long LinkerData = 0x0018; // 32 bytes
        public const long ChunkTableOffset = 0x0038; // 4 bytes
        public const long GlobalListOffset = 0x003C; // 4 bytes
    }
}
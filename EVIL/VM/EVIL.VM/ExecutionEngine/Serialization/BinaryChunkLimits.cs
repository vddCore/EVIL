namespace EVIL.VM.ExecutionEngine.Serialization;

using System.Text;

public static class BinaryChunkLimits
{
    public const int HeaderSize = 28;
    public const int DirectoryEntrySize = 12;
    public const int DigestSize = 32;
    public const int MaximumFrameSize = 64 * 1024 * 1024;
    public const int MaximumSectionCount = 64;
    public const int MaximumItemCount = 1000000;
    public const int MaximumStringSize = 1024 * 1024;
    public const int MaximumNestingDepth = 64;
    public const int MinimumFrameSize = HeaderSize + 3 * DirectoryEntrySize + DigestSize;

    public const uint KnownChunkFlags = (1u << 13) - 1;

    public static readonly Encoding UTF8 = new UTF8Encoding(false, true);
}
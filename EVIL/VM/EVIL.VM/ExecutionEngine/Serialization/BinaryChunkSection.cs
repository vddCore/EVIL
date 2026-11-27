namespace EVIL.VM.ExecutionEngine.Serialization;

public readonly record struct BinaryChunkSection(
    ushort Tag,
    byte[] Data
);

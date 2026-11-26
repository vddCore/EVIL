namespace EVIL.VirtualMachine.ExecutionEngine.Diagnostics.Disassembly;

public static class DisassemblyExtensions
{
    public static ChunkDisassembly Disassemble(this Chunk chunk)
        => InstructionDecoder.Decode(chunk);

    public static ChunkDisassembly GetDisassembly(this Chunk chunk)
        => InstructionDecoder.Decode(chunk);
}

namespace EVIL.VirtualMachine.ExecutionEngine.Diagnostics;

public readonly struct CallFrame(Chunk chunk, Upvalue[]? upvalues, int basePointer, int returnIp)
{
    public readonly Chunk Chunk = chunk;
    public readonly Upvalue[]? Upvalues = upvalues;
    public readonly int BasePointer = basePointer;
    public readonly int ReturnIP = returnIp;
}

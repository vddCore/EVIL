namespace EVIL.VirtualMachine.ExecutionEngine.Diagnostics.Debugging;

using EVIL.VirtualMachine.ExecutionEngine.Concurrency;

public delegate void ChunkInvokeHandler(Fiber fiber, Chunk chunk, bool isTailCall);
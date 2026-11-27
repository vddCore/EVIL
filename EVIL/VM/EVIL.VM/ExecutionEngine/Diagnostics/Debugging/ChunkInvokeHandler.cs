namespace EVIL.VM.ExecutionEngine.Diagnostics.Debugging;

using EVIL.VM.ExecutionEngine.Concurrency;

public delegate void ChunkInvokeHandler(Fiber fiber, Chunk chunk, bool isTailCall);
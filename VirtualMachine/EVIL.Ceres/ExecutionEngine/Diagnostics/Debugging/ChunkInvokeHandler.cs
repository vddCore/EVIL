namespace EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

using EVIL.Ceres.ExecutionEngine.Concurrency;

public delegate void ChunkInvokeHandler(Fiber fiber, Chunk chunk, bool isTailCall);
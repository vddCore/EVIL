using EVIL.Ceres.ExecutionEngine.Concurrency;

namespace EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging
{
    public delegate void ChunkInvokeHandler(Fiber fiber, Chunk chunk, bool isTailCall);
}
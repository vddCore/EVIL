using Ceres.ExecutionEngine.Concurrency;

namespace Ceres.ExecutionEngine.Diagnostics.Debugging
{
    public delegate void ChunkInvokeHandler(Fiber fiber, Chunk chunk, bool isTailCall);
}
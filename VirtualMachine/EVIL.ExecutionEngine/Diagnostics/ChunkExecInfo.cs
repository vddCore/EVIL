using EVIL.ExecutionEngine.Abstraction;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.ExecutionEngine.Diagnostics
{
    internal class ChunkExecInfo
    {
        public Chunk Chunk { get; }
        public DynamicValue[] Arguments { get; }
        
        public ChunkExecInfo(Chunk chunk, DynamicValue[] arguments)
        {
            Chunk = chunk;
            Arguments = arguments;
        }
    }
}
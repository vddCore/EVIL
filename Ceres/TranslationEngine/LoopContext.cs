using Ceres.ExecutionEngine.Diagnostics;

namespace Ceres.TranslationEngine
{
    public class LoopContext
    {
        public Chunk Chunk { get; }
        public int StartLabel { get; }
        public int EndLabel { get; }

        internal LoopContext(Chunk chunk)
        {
            Chunk = chunk;

            StartLabel = chunk.CreateLabel();
            EndLabel = chunk.CreateLabel();
        }
    }
}
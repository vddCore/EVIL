using Ceres.ExecutionEngine.Diagnostics;

namespace Ceres.TranslationEngine
{
    public class Loop
    {
        public Chunk Chunk { get; }
        
        public int StartLabel { get; }
        public int EndLabel { get; }

        internal Loop(Chunk chunk)
        {
            Chunk = chunk;

            StartLabel = chunk.CreateLabel();
            EndLabel = chunk.CreateLabel();
        }
    }
}
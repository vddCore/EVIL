using Ceres.ExecutionEngine.Diagnostics;

namespace Ceres.TranslationEngine
{
    internal class Loop
    {
        public Chunk Chunk { get; }

        public int StartLabel { get; }
        public int EndLabel { get; }
        public int ExtraLabel { get; }

        internal Loop(Chunk chunk, bool needsExtraLabel)
        {
            Chunk = chunk;

            StartLabel = Chunk.CreateLabel();
            EndLabel = Chunk.CreateLabel();

            if (needsExtraLabel)
            {
                ExtraLabel = Chunk.CreateLabel();
            }
        }
    }
}
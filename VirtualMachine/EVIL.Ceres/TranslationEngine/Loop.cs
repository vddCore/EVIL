using EVIL.Ceres.ExecutionEngine.Diagnostics;

namespace EVIL.Ceres.TranslationEngine
{
    internal class Loop
    {
        public enum LoopKind
        {
            For,
            While,
            DoWhile,
            Each
        }
        
        public Chunk Chunk { get; }
        public LoopKind Kind { get; }

        public int StartLabel { get; }
        public int EndLabel { get; }
        public int ExtraLabel { get; }

        internal Loop(Chunk chunk, LoopKind kind, bool needsExtraLabel)
        {
            Chunk = chunk;
            Kind = kind;
            
            StartLabel = Chunk.CreateLabel();
            EndLabel = Chunk.CreateLabel();

            if (needsExtraLabel)
            {
                ExtraLabel = Chunk.CreateLabel();
            }
        }
    }
}
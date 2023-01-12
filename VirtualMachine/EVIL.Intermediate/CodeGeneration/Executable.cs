using System.Collections.Generic;

namespace EVIL.Intermediate.CodeGeneration
{
    public class Executable
    {
        public List<string> Globals { get; } = new();
        public List<Chunk> Chunks { get; } = new();
        
        public Chunk RootChunk => Chunks[0];
    }
}
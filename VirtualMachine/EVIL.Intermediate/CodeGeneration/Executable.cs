using System.Collections.Generic;

namespace EVIL.Intermediate.CodeGeneration
{
    public class Executable
    {
        public List<string> Globals { get; } = new();
        public ConstPool ConstPool = new();

        public List<Chunk> Chunks { get; } = new();
        public Chunk RootChunk => Chunks[0];

        public Executable(bool createRootChunk = true)
        {
            if (createRootChunk)
                Chunks.Add(new Chunk("!root"));
        }
    }
}
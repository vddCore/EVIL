using System.Collections.Generic;

namespace EVIL.Intermediate
{
    public class Executable
    {        
        public List<string> Globals { get; } = new();
        public ConstPool ConstPool = new();
        
        public List<Chunk> Chunks { get; } = new();
        public Chunk RootChunk => Chunks[0];

        public Executable()
        {
            Chunks.Add(new Chunk("!root"));
        }
    }
}
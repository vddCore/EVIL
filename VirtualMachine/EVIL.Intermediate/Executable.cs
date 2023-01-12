using System.Collections.Generic;

namespace EVIL.Intermediate
{
    public class Executable
    {
        public ConstPool ConstPool = new();
        
        public List<Chunk> Chunks { get; } = new();
        
        public Chunk MainChunk => Chunks[0];

        public Executable()
        {
            Chunks.Add(new Chunk("!mainch"));
        }
    }
}
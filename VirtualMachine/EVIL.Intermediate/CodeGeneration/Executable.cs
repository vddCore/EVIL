using System.Collections.Generic;
using System.Linq;

namespace EVIL.Intermediate.CodeGeneration
{
    public class Executable
    {
        public List<Chunk> Chunks { get; } = new();
        public Chunk RootChunk => Chunks.Single(c => c.IsRoot);
        
        public Chunk FindExposedChunk(string funcName)
        {
            for (var i = 0; i < Chunks.Count; i++)
            {
                var ch = Chunks[i];

                if (ch.IsRoot || !ch.IsPublic)
                    continue;

                if (ch.Name == funcName)
                {
                    return ch;
                }
            }

            return null;
        }
    }
}
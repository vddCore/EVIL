using System.Collections.Generic;

namespace EVIL.Intermediate
{
    public class Executable
    {
        private int _nextAnonymousChunkId;
        
        public List<string> Globals { get; } = new();
        public ConstPool ConstPool = new();
        
        public List<Chunk> Chunks { get; } = new();
        public Chunk RootChunk => Chunks[0];

        public Executable()
        {
            Chunks.Add(new Chunk("!root"));
        }

        public (int, Chunk) CreateAnonymousChunk()
        {
            var id = Chunks.Count;
            var chunk = new Chunk($"!!<{_nextAnonymousChunkId++}>");

            Chunks.Add(chunk);

            return (id, chunk);
        }
        
        public void DefineGlobal(string name)
        {
            if (IsGlobalDefined(name))
                throw new DuplicateSymbolException(name);

            Globals.Add(name);
        }

        public bool IsGlobalDefined(string name)
        {
            return Globals.Contains(name);
        }
    }
}
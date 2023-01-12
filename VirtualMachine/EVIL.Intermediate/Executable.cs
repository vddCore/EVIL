using System.Collections.Generic;

namespace EVIL.Intermediate
{
    public class Executable
    {
        public List<string> Globals { get; } = new();
        public ConstPool ConstPool = new();
        public List<Chunk> Chunks { get; } = new();
        
        public Chunk MainChunk => Chunks[0];

        public Executable()
        {
            Chunks.Add(new Chunk("!root"));
        }
        
        public void DefineGlobal(string name)
        {
            if (IsGlobalDefined(name))
                throw new DuplicateSymbolException($"Global symbol '{name}' was already defined.", name);

            Globals.Add(name);
        }

        public bool IsGlobalDefined(string name)
        {
            return Globals.Contains(name);
        }
    }
}
using System.Collections.Generic;

namespace EVIL.Intermediate
{
    public class Executable
    {
        public List<string> Globals { get; } = new();
        public ConstPool ConstPool = new();
        public List<Chunk> Chunks { get; } = new();
        public List<int> Labels { get; } = new();
        
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
        
        public int DefineLabel(int address = 0)
        {
            Labels.Add(address);
            return Labels.Count - 1;
        }

        public void UpdateLabel(int id, int address)
        {
            Labels[id] = address;
        }

        public bool IsGlobalDefined(string name)
        {
            return Globals.Contains(name);
        }
    }
}
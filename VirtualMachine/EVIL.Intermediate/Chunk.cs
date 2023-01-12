using System.Collections.Generic;

namespace EVIL.Intermediate
{
    public class Chunk
    {
        private CodeGenerator _codeGenerator;

        public string Name { get; }
        public List<byte> Instructions { get; } = new();
        public List<int> Labels { get; } = new();
        public List<string> Parameters { get; } = new();
        public List<string> Locals { get; } = new();

        public Chunk(string name)
        {
            Name = name;
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

        public CodeGenerator GetCodeGenerator()
            => _codeGenerator ??= new CodeGenerator(this);
    }
}
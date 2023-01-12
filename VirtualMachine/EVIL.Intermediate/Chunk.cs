using System.Collections.Generic;

namespace EVIL.Intermediate
{
    public class Chunk
    {
        private CodeGenerator _codeGenerator;

        public string Name { get; private set; }
        public List<byte> Instructions { get; private set; } = new();
        public List<int> Labels { get; private set; } = new();
        public List<string> Parameters { get; private set; } = new();
        public List<string> Locals { get; private set; } = new();
        public List<ExternInfo> Externs { get; private set; } = new();
        
        private Chunk()
        {
        }
        
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

        public Chunk ShallowClone()
        {
            var c = new Chunk
            {
                Name = Name,
                Instructions = Instructions,
                Labels = Labels,
                Parameters = Parameters,
                Locals = Locals,
                Externs = Externs
            };

            return c;
        }
    }
}
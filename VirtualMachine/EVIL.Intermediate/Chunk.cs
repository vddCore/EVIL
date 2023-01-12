using System.Collections.Generic;

namespace EVIL.Intermediate
{
    public class Chunk
    {
        private CodeGenerator _codeGenerator;

        public string Name { get; }
        public List<byte> Instructions { get; } = new();
        
        public int ParameterCount { get; internal set; }
        public int LocalCount { get; internal set; }

        public Chunk(string name)
        {
            Name = name;
        }

        public CodeGenerator GetCodeGenerator()
            => _codeGenerator ??= new CodeGenerator(this);
    }
}
using System.Collections.Generic;
using System.Diagnostics;

namespace EVIL.Intermediate.CodeGeneration
{
    [DebuggerDisplay("{Name}")]
    public class Chunk
    {
        private CodeGenerator _codeGenerator;

        public string Name { get; private set; }
        public bool IsPublic { get; private set; }
        public List<int> Labels { get; private set; } = new();
        public List<string> Parameters { get; private set; } = new();
        public List<string> Locals { get; private set; } = new();
        public List<ExternInfo> Externs { get; private set; } = new();
        public List<Chunk> SubChunks { get; private set; } = new();
        public List<byte> Instructions { get; private set; } = new();
        public ConstPool Constants { get; private set; } = new();
        
        public HashSet<DebugEntry> DebugInfo { get; private set; } = new();

        private Chunk()
        {
        }

        public (int, int) GetCodeCoordinatesForInstructionPointer(int ip)
        {
            foreach (var de in DebugInfo)
            {
                if (ip <= de.IP)
                    return (de.Line, de.Column);
            }

            return (-1, -1);
        }

        public Chunk(string name, bool isPublic = true)
        {
            Name = name;
            IsPublic = isPublic;
        }

        public (int, Chunk) CreateSubChunk()
        {
            var id = SubChunks.Count;
            var chunk = new Chunk($"<unnamed_anon_chunk>", false);

            SubChunks.Add(chunk);

            return (id, chunk);
        }

        public void Rename(string newName)
        {
            Name = newName;
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
                Externs = Externs,
                Constants = Constants,
                SubChunks = SubChunks,
                DebugInfo = DebugInfo
            };

            return c;
        }
    }
}
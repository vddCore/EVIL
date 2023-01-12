using System.Collections.Generic;
using System.Text;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.Intermediate.Analysis
{
    public class Disassembler
    {
        private StringBuilder _disasm = new();

        private int IP { get; set; }

        public DisassemblerOptions Options { get; }

        public Disassembler(DisassemblerOptions options = null)
        {
            Options = options ?? new DisassemblerOptions();

        }

        public string Disassemble(Executable executable)
        {
            _disasm.Clear();

            DumpGlobalList(executable.Globals);

            for(var ci = 0; ci < executable.Chunks.Count; ci++)
            {
                var chunk = executable.Chunks[ci];
                _disasm.AppendLine(new ChunkDisassembler(chunk, Options, false).ToString());
            }

            return _disasm.ToString();
        }

        private void DumpGlobalList(List<string> globals)
        {
            for (var i = 0; i < globals.Count; i++)
            {
                _disasm.AppendLine($" .GLOBAL {i} ; {globals[i]}");
            }
        }
    }
}
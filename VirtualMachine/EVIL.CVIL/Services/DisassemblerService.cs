using System;
using System.Collections.Generic;
using EVIL.Intermediate.Analysis;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.CVIL.Services
{
    public class DisassemblerService : Service
    {
        private readonly Disassembler _disassembler;

        public bool Enabled { get; set; } = false;

        public DisassemblerService(DisassemblerOptions options = null)
        {
            _disassembler = new Disassembler(options ?? new DisassemblerOptions
            {
                EmitLineNumbers = false
            });
        }

        public void Disassemble(IEnumerable<Chunk> chunks)
        {
            if (!Enabled) return;
            
            Console.WriteLine(_disassembler.Disassemble(chunks));
        }
    }
}
using System.Collections.Generic;
using EVIL.Intermediate.CodeGeneration;
using EvilProgram = EVIL.Grammar.AST.Nodes.Program;

namespace EVIL.CVIL.Services
{
    public class CompilerService : Service
    {
        private CompilerOptions _compilerOptions;
        private readonly Compiler _compiler;

        public bool OptimizeBytecode
        {
            get => _compilerOptions.OptimizeBytecode;
            set => _compilerOptions.OptimizeBytecode = value;
        }

        public bool GenerateDebugInfo
        {
            get => _compilerOptions.GenerateDebugInformation;
            set => _compilerOptions.GenerateDebugInformation = value;
        }
        
        public CompilerService(CompilerOptions compilerOptions = null)
        {
            _compilerOptions = compilerOptions ?? new CompilerOptions
            {
                GenerateDebugInformation = true,
                OptimizeBytecode = false
            };
            
            _compiler = new Compiler(_compilerOptions);
        }

        public Executable Compile(EvilProgram program)
            => _compiler.Compile(program);
    }
}
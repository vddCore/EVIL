using Ceres.ExecutionEngine.Diagnostics;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        private void EmitVarSet(string identifier)
        {
            var sym = _currentScope.Find(identifier);

            if (sym != null)
            {
                if (sym.Type == Symbol.SymbolType.Local)
                {
                    Chunk.CodeGenerator.Emit(
                        OpCode.SETLOCAL,
                        sym.Id
                    );
                }
                else
                {
                    Chunk.CodeGenerator.Emit(
                        OpCode.SETARG,
                        sym.Id
                    );
                }
            }
            else
            {
                Chunk.CodeGenerator.Emit(
                    OpCode.LDSTR,
                    (int)Chunk.StringPool.FetchOrAdd(identifier)
                );

                Chunk.CodeGenerator.Emit(OpCode.SETGLOBAL);
            }
        }
        
        private void EmitVarGet(string identifier)
        {
            var sym = _currentScope.Find(identifier);

            if (sym != null)
            {
                if (sym.Type == Symbol.SymbolType.Local)
                {
                    Chunk.CodeGenerator.Emit(
                        OpCode.GETLOCAL,
                        sym.Id
                    );
                }
                else
                {
                    Chunk.CodeGenerator.Emit(
                        OpCode.GETARG,
                        sym.Id
                    );
                }
            }
            else
            {
                Chunk.CodeGenerator.Emit(
                    OpCode.LDSTR,
                    (int)Chunk.StringPool.FetchOrAdd(identifier)
                );

                Chunk.CodeGenerator.Emit(OpCode.GETGLOBAL);
            }
        }
    }
}
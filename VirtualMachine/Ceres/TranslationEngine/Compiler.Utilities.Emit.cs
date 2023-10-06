﻿using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Scoping;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        private void EmitVarSet(string identifier)
        {
            var sym = CurrentScope.Find(identifier);

            if (sym != null)
            {
                switch (sym.Type)
                {
                    case Symbol.SymbolType.Local:
                    {
                        Chunk.CodeGenerator.Emit(
                            OpCode.SETLOCAL,
                            sym.Id
                        );
                        break;
                    }

                    case Symbol.SymbolType.Parameter:
                    {
                        Chunk.CodeGenerator.Emit(
                            OpCode.SETARG,
                            sym.Id
                        );
                        break;
                    }
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
            var sym = CurrentScope.Find(identifier);

            if (sym != null)
            {
                switch (sym.Type)
                {
                    case Symbol.SymbolType.Local:
                    {
                        Chunk.CodeGenerator.Emit(
                            OpCode.GETLOCAL,
                            sym.Id
                        );
                        break;
                    }

                    case Symbol.SymbolType.Parameter:
                    {
                        Chunk.CodeGenerator.Emit(
                            OpCode.GETARG,
                            sym.Id
                        );
                        break;
                    }
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
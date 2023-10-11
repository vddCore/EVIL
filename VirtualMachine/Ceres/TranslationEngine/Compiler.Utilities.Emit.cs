using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Scoping;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        private void EmitVarSet(string identifier)
        {
            var symbolInfo = FindSymbolInClosedScopes(identifier);

            if (symbolInfo != null)
            {
                var ownerScope = symbolInfo.Value.OwnerScope;
                var level = symbolInfo.Value.ClosedScopeLevel;
                var sym = symbolInfo.Value.Symbol;

                if (level == 0)
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

                        case Symbol.SymbolType.Closure:
                        {
                            Chunk.CodeGenerator.Emit(
                                OpCode.SETCLOSURE,
                                sym.Id
                            );
                            break;
                        }
                    }
                }
                else
                {
                    var result = Chunk.AllocateClosure(
                        level,
                        sym.Id,
                        ownerScope.FunctionName,
                        sym.Type == Symbol.SymbolType.Parameter,
                        sym.Type == Symbol.SymbolType.Closure
                    );

                    CurrentScope.DefineClosure(
                        sym.Name,
                        result.Id,
                        sym.ReadWrite,
                        sym.DefinedOnLine,
                        sym.DefinedOnColumn,
                        result.Closure
                    );

                    Chunk.CodeGenerator.Emit(
                        OpCode.SETCLOSURE,
                        result.Id
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
            var symbolInfo = FindSymbolInClosedScopes(identifier);

            if (symbolInfo != null)
            {
                var ownerScope = symbolInfo.Value.OwnerScope;
                var level = symbolInfo.Value.ClosedScopeLevel;
                var sym = symbolInfo.Value.Symbol;

                if (level == 0)
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

                        case Symbol.SymbolType.Closure:
                        {
                            Chunk.CodeGenerator.Emit(
                                OpCode.GETCLOSURE,
                                sym.Id
                            );

                            break;
                        }
                    }
                }
                else
                {
                    var result = Chunk.AllocateClosure(
                        level,
                        sym.Id,
                        ownerScope.FunctionName,
                        sym.Type == Symbol.SymbolType.Parameter,
                        sym.Type == Symbol.SymbolType.Closure
                    );

                    CurrentScope.DefineClosure(
                        sym.Name,
                        result.Id,
                        sym.ReadWrite,
                        sym.DefinedOnLine,
                        sym.DefinedOnColumn,
                        result.Closure
                    );

                    Chunk.CodeGenerator.Emit(
                        OpCode.GETCLOSURE,
                        result.Id
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

        private (int ClosedScopeLevel, Symbol Symbol, Scope OwnerScope)? FindSymbolInClosedScopes(string identifier)
        {
            for (var i = 0; i < _closedScopes.Count; i++)
            {
                var scope = _closedScopes[i];
                var result = scope.Find(identifier);

                if (result != null)
                {
                    return (i, result.Value.Symbol, scope);
                }
            }

            return null;
        }
    }
}
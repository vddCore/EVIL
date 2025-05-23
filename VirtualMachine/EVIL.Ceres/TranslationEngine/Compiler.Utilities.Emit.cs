﻿namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine.Scoping;

public partial class Compiler
{
    private void FinalizeChunk()
    {
        if (Chunk.CodeGenerator.TryPeekOpCode(out var opCode))
        {
            if (opCode is OpCode.RET 
                       or OpCode.TAILINVOKE
                       or OpCode.THROW)
            {
                return;
            }
        }

        /* Either we have no instructions in chunk, or it's not a RET nor THROW. */
        Chunk.CodeGenerator.Emit(OpCode.LDNIL);
        Chunk.CodeGenerator.Emit(OpCode.RET);
    }
        
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
                ClosureType closureType;
                switch (sym.Type)
                {
                    case Symbol.SymbolType.Parameter:
                        closureType = ClosureType.Parameter;
                        break;
                    case Symbol.SymbolType.Local:
                        closureType = ClosureType.Local;
                        break;
                    case Symbol.SymbolType.Closure:
                        closureType = ClosureType.Closure;
                        break;
                    default:
                        Log.TerminateWithInternalFailure(
                            $"Invalid type '{sym.Type}' for symbol '{identifier}'.",
                            CurrentFileName,
                            line: Line,
                            column: Column,
                            dummyReturn: OpCode.NOOP
                        );
                        return;
                }

                var result = Chunk.AllocateClosure(
                    level,
                    sym.Id,
                    ownerScope.Chunk.Name,
                    closureType,
                    ReferenceEquals(ownerScope.Chunk, _rootChunk)
                );

                try
                {
                    CurrentScope.DefineClosure(
                        sym.Name,
                        result.Id,
                        sym.ReadWrite,
                        sym.DefinedOnLine,
                        sym.DefinedOnColumn,
                        result.Closure
                    );
                }
                catch (DuplicateSymbolException dse)
                {
                    Log.TerminateWithFatal(
                        $"The symbol '{sym.Name}' already exists in this scope " +
                        $"and is a {dse.ExistingSymbol.TypeName} (previously defined on line {dse.Line}, column {dse.Column}).",
                        CurrentFileName,
                        EvilMessageCode.DuplicateSymbolInScope,
                        sym.DefinedOnLine,
                        sym.DefinedOnColumn,
                        dse
                    );
                }

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
                ClosureType closureType;
                switch (sym.Type)
                {
                    case Symbol.SymbolType.Parameter:
                        closureType = ClosureType.Parameter;
                        break;
                    case Symbol.SymbolType.Local:
                        closureType = ClosureType.Local;
                        break;
                    case Symbol.SymbolType.Closure:
                        closureType = ClosureType.Closure;
                        break;
                    default:
                        Log.TerminateWithInternalFailure(
                            $"Invalid type '{sym.Type}' for symbol '{identifier}'.",
                            CurrentFileName,
                            line: Line,
                            column: Column,
                            dummyReturn: OpCode.NOOP
                        );
                        return;
                }
                
                var result = Chunk.AllocateClosure(
                    level,
                    sym.Id,
                    ownerScope.Chunk.Name,
                    closureType,
                    ReferenceEquals(ownerScope.Chunk, _rootChunk)
                );

                try
                {
                    CurrentScope.DefineClosure(
                        sym.Name,
                        result.Id,
                        sym.ReadWrite,
                        sym.DefinedOnLine,
                        sym.DefinedOnColumn,
                        result.Closure
                    );
                }
                catch (DuplicateSymbolException dse)
                {
                    Log.TerminateWithFatal(
                        $"The symbol '{sym.Name}' already exists in this scope " +
                        $"and is a {dse.ExistingSymbol.TypeName} (previously defined on line {dse.Line}, column {dse.Column}).",
                        CurrentFileName,
                        EvilMessageCode.DuplicateSymbolInScope,
                        sym.DefinedOnLine,
                        sym.DefinedOnColumn,
                        dse
                    );
                }

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

    private (int ClosedScopeLevel, Scoping.Symbol Symbol, Scope OwnerScope)? FindSymbolInClosedScopes(string identifier)
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
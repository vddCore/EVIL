namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine.Scoping;
using EVIL.Diagnostics;
using EVIL.Grammar;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    private void EmitConditionBranch(Expression condition, int trueLabel, int falseLabel, bool trueIsFallthrough)
    {
        if (condition is BinaryExpression { Type: BinaryOperationType.LogicalAnd } andBex)
        {
            var rhsLabel = Chunk.CreateLabel();
            EmitConditionBranch(andBex.Left, rhsLabel, falseLabel, true);
            
            Chunk.UpdateLabel(rhsLabel, Chunk.CodeGenerator.IP);
            EmitConditionBranch(andBex.Right, trueLabel, falseLabel, trueIsFallthrough);
        }
        else if (condition is BinaryExpression { Type: BinaryOperationType.LogicalOr } orBex)
        {
            var rhsLabel = Chunk.CreateLabel();
            EmitConditionBranch(orBex.Left, trueLabel, rhsLabel, false);
            
            Chunk.UpdateLabel(rhsLabel, Chunk.CodeGenerator.IP);
            EmitConditionBranch(orBex.Right, trueLabel, falseLabel, trueIsFallthrough);
        }
        else
        {
            Visit(condition);
            if (trueIsFallthrough)
            {
                Chunk.CodeGenerator.Emit(OpCode.FJMP, falseLabel);
            }
            else
            {
                Chunk.CodeGenerator.Emit(OpCode.TJMP, trueLabel);
            }
        }
    }

    private void FinalizeChunk()
    {
        if (!_isUnreachable)
        {
            Chunk.CodeGenerator.Emit(OpCode.LDNIL);
            Chunk.CodeGenerator.Emit(OpCode.RET);
        }

        Chunk.BackpatchLabels();
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
                    {
                        Log.TerminateWithInternalFailure(
                            CompilerDiagnostics.InternalCompilerError.Create(
                                GetLocation(Line, Column),
                                $"Invalid type '{sym.Type}' for symbol '{identifier}'."
                            ),
                            dummyReturn: OpCode.NOOP
                        );
                        return;
                    }
                }

                var result = Chunk.AllocateClosure(
                    level,
                    sym.Id,
                    ownerScope.Chunk.Name,
                    closureType,
                    ReferenceEquals(ownerScope.Chunk, _rootChunk),
                    ownerScope.Chunk.ClosureOwnerId
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
                        CompilerDiagnostics.DuplicateSymbolInScope.Create(
                            GetLocation(Line, Column),
                            [new RelatedLocation(new SourceLocation(CurrentFileName, dse.Line, dse.Column), "Previously defined here")],
                            sym.Name,
                            dse.ExistingSymbol.TypeName,
                            dse.Line,
                            dse.Column
                        ),
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
                    {
                        Log.TerminateWithInternalFailure(
                            CompilerDiagnostics.InternalCompilerError.Create(
                                GetLocation(Line, Column),
                                $"Invalid type '{sym.Type}' for symbol '{identifier}'."
                            ),
                            dummyReturn: OpCode.NOOP
                        );
                        return;
                    }
                }
                
                var result = Chunk.AllocateClosure(
                    level,
                    sym.Id,
                    ownerScope.Chunk.Name,
                    closureType,
                    ReferenceEquals(ownerScope.Chunk, _rootChunk),
                    ownerScope.Chunk.ClosureOwnerId
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
                        CompilerDiagnostics.DuplicateSymbolInScope.Create(
                            GetLocation(Line, Column),
                            [new RelatedLocation(new SourceLocation(CurrentFileName, dse.Line, dse.Column), "Previously defined here")],
                            sym.Name,
                            dse.ExistingSymbol.TypeName,
                            dse.Line,
                            dse.Column
                        ),
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
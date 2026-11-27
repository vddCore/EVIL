namespace EVIL.VM.TranslationEngine;

using System;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Miscellaneous;
using EVIL.Core.Grammar.AST.Statements;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.TranslationEngine.Scoping;

public partial class Compiler
{
    protected override void Visit(ValStatement valStatement)
    {
        foreach (var declarator in valStatement.Declarators)
        {
            if (declarator is IdentifierValDeclarator identifierDeclarator)
            {
                var symbol = RegisterValSymbol(
                    valStatement,
                    identifierDeclarator.Identifier,
                    valStatement.ReadWrite
                );

                if (identifierDeclarator.Initializer != null)
                {
                    Visit(identifierDeclarator.Initializer);
                    EmitValBindingSet(symbol);
                }
            }
            else if (declarator is ArrayPatternValDeclarator arrayPattern)
            {
                CompileArrayPattern(valStatement, arrayPattern);
            }
            else if (declarator is TablePatternValDeclarator tablePattern)
            {
                CompileTablePattern(valStatement, tablePattern);
            }
        }
    }

    private void CompileArrayPattern(
        ValStatement valStatement,
        ArrayPatternValDeclarator arrayPattern)
    {
        var symbols = new Symbol[arrayPattern.Bindings.Count];
        for (var i = 0; i < arrayPattern.Bindings.Count; i++)
        {
            var binding = arrayPattern.Bindings[i];
            symbols[i] = RegisterValSymbol(
                valStatement,
                binding.Identifier,
                valStatement.ReadWrite || binding.ReadWrite
            );
        }

        Visit(arrayPattern.Initializer!);

        Line = valStatement.Line;
        Column = valStatement.Column;
        Chunk.CodeGenerator.Emit(OpCode.ARRDEC, arrayPattern.Bindings.Count);
        StoreValBindings(symbols);
    }

    private void CompileTablePattern(
        ValStatement valStatement,
        TablePatternValDeclarator tablePattern)
    {
        var symbols = new Symbol[tablePattern.Fields.Count];
        for (var i = 0; i < tablePattern.Fields.Count; i++)
        {
            var binding = tablePattern.Fields[i].Binding;
            symbols[i] = RegisterValSymbol(
                valStatement,
                binding.Identifier,
                valStatement.ReadWrite || binding.ReadWrite
            );
        }

        var sourceLocal = Chunk.AllocateLocal();
        Visit(tablePattern.Initializer!);

        Line = valStatement.Line;
        Column = valStatement.Column;
        Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, sourceLocal);

        foreach (var field in tablePattern.Fields)
        {
            Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, sourceLocal);
            var key = (int)Chunk.StringPool.FetchOrAdd(field.Key);
            Chunk.CodeGenerator.EmitGetTableField(key);
        }

        StoreValBindings(symbols);
    }

    private void StoreValBindings(Symbol[] symbols)
    {
        if (symbols[0].Type != Symbol.SymbolType.Global)
        {
            for (var i = symbols.Length - 1; i >= 0; i--)
            {
                Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, symbols[i].Id);
            }

            return;
        }

        var temporaryIds = new int[symbols.Length];
        for (var i = 0; i < temporaryIds.Length; i++)
        {
            temporaryIds[i] = Chunk.AllocateLocal();
        }

        for (var i = temporaryIds.Length - 1; i >= 0; i--)
        {
            Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, temporaryIds[i]);
        }

        for (var i = 0; i < temporaryIds.Length; i++)
        {
            Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, temporaryIds[i]);
            EmitVarSet(symbols[i].Name);
        }
    }

    private void EmitValBindingSet(Symbol symbol)
    {
        if (symbol.Type == Symbol.SymbolType.Global)
        {
            EmitVarSet(symbol.Name);
        }
        else
        {
            Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, symbol.Id);
        }
    }

    private Symbol RegisterValSymbol(
        ValStatement valStatement,
        IdentifierNode identifier,
        bool readWrite)
    {
        if (_replSymbols != null
            && ReferenceEquals(Chunk, _rootChunk)
            && CurrentScope.Parent == null)
        {
            if (_replSymbols.TryGetValue(identifier.Name, out var existingSymbol))
            {
                Log.TerminateWithFatal(
                    CompilerDiagnostics.DuplicateSymbolInScope.Create(
                        GetLocation(identifier),
                        [new RelatedLocation(
                            new SourceLocation(CurrentFileName, existingSymbol.DefinedOnLine, existingSymbol.DefinedOnColumn),
                            "Previously defined here"
                        )],
                        identifier.Name,
                        existingSymbol.TypeName,
                        existingSymbol.DefinedOnLine,
                        existingSymbol.DefinedOnColumn
                    )
                );
            }

            var globalSymbol = new Symbol(
                identifier.Name,
                -1,
                Symbol.SymbolType.Global,
                readWrite,
                valStatement.Line,
                valStatement.Column,
                null
            );

            _replSymbols.Add(identifier.Name, globalSymbol);
            return globalSymbol;
        }

        var localId = Chunk.AllocateLocal();

        try
        {
            var symbol = CurrentScope.DefineLocal(
                identifier.Name,
                localId,
                readWrite,
                valStatement.Line,
                valStatement.Column
            );

            Chunk.DebugDatabase.SetLocalName(localId, identifier.Name, readWrite);
            return symbol;
        }
        catch (DuplicateSymbolException dse)
        {
            Log.TerminateWithFatal(
                CompilerDiagnostics.DuplicateSymbolInScope.Create(
                    GetLocation(identifier),
                    [new RelatedLocation(
                        new SourceLocation(CurrentFileName, dse.Line, dse.Column),
                        "Previously defined here"
                    )],
                    identifier.Name,
                    dse.ExistingSymbol.TypeName,
                    dse.Line,
                    dse.Column
                ),
                dse
            );

            // Return just in case - TerminateWithFatal should never return, ever.
        }

        throw new InvalidOperationException("Unreachable code after a fatal compiler diagnostic.");
    }
}
namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Miscellaneous;
using EVIL.Core.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(ValStatement valStatement)
    {
        foreach (var declarator in valStatement.Declarators)
        {
            if (declarator is IdentifierValDeclarator identifierDeclarator)
            {
                var localId = RegisterValLocal(
                    valStatement,
                    identifierDeclarator.Identifier,
                    valStatement.ReadWrite
                );

                if (identifierDeclarator.Initializer != null)
                {
                    Visit(identifierDeclarator.Initializer);
                    Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, localId);
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
        var localIds = new int[arrayPattern.Bindings.Count];
        for (var i = 0; i < arrayPattern.Bindings.Count; i++)
        {
            var binding = arrayPattern.Bindings[i];
            localIds[i] = RegisterValLocal(
                valStatement,
                binding.Identifier,
                valStatement.ReadWrite || binding.ReadWrite
            );
        }

        Visit(arrayPattern.Initializer!);

        Line = valStatement.Line;
        Column = valStatement.Column;
        Chunk.CodeGenerator.Emit(OpCode.ARRDEC, arrayPattern.Bindings.Count);

        for (var i = localIds.Length - 1; i >= 0; i--)
        {
            Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, localIds[i]);
        }
    }

    private void CompileTablePattern(
        ValStatement valStatement,
        TablePatternValDeclarator tablePattern)
    {
        var localIds = new int[tablePattern.Fields.Count];
        for (var i = 0; i < tablePattern.Fields.Count; i++)
        {
            var binding = tablePattern.Fields[i].Binding;
            localIds[i] = RegisterValLocal(
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

        for (var i = localIds.Length - 1; i >= 0; i--)
        {
            Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, localIds[i]);
        }
    }

    private int RegisterValLocal(
        ValStatement valStatement,
        IdentifierNode identifier,
        bool readWrite)
    {
        var localId = Chunk.AllocateLocal();

        try
        {
            CurrentScope.DefineLocal(
                identifier.Name,
                localId,
                readWrite,
                valStatement.Line,
                valStatement.Column
            );
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
            return localId;
        }

        Chunk.DebugDatabase.SetLocalName(localId, identifier.Name, readWrite);
        return localId;
    }
}
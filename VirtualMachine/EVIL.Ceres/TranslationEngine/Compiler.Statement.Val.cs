namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Diagnostics;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(ValStatement valStatement)
    {
        foreach (var kvp in valStatement.Definitions)
        {
            var localId = Chunk.AllocateLocal();

            try
            {
                CurrentScope.DefineLocal(
                    kvp.Key.Name,
                    localId,
                    valStatement.ReadWrite,
                    valStatement.Line,
                    valStatement.Column
                );
            }
            catch (DuplicateSymbolException dse)
            {
                Log.TerminateWithFatal(
                    CompilerDiagnostics.DuplicateSymbolInScope.Create(
                        GetLocation(kvp.Key),
                        [new RelatedLocation(
                            new SourceLocation(CurrentFileName, dse.Line, dse.Column),
                            "Previously defined here"
                        )],
                        kvp.Key.Name,
                        dse.ExistingSymbol.TypeName,
                        dse.Line,
                        dse.Column
                    ),
                    dse
                );

                // Return just in case - TerminateWithFatal should never return, ever.
                return;
            }

            Chunk.DebugDatabase.SetLocalName(localId, kvp.Key.Name, valStatement.ReadWrite);

            if (kvp.Value != null)
            {
                Visit(kvp.Value);

                Chunk.CodeGenerator.Emit(
                    OpCode.SETLOCAL,
                    localId
                );
            }
        }
    }
}
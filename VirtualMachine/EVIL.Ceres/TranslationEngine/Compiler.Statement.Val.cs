using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ValStatement valStatement)
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
                        $"The symbol '{kvp.Key.Name}' already exists in this scope " +
                        $"and is a {dse.ExistingSymbol.TypeName} (previously defined on line {dse.Line}, column {dse.Column}).",
                        CurrentFileName,
                        EvilMessageCode.DuplicateSymbolInScope,
                        kvp.Key.Line,
                        kvp.Key.Column,
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
}
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
using Ceres.TranslationEngine.Scoping;
using EVIL.Grammar.AST.Statements;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ValStatement valStatement)
        {
            foreach (var kvp in valStatement.Definitions)
            {
                Symbol sym;
                try
                {
                    var localId = Chunk.AllocateLocal();

                    sym = CurrentScope.DefineLocal(
                        kvp.Key.Name,
                        localId,
                        valStatement.ReadWrite,
                        valStatement.Line,
                        valStatement.Column
                    );

                    Chunk.DebugDatabase.SetLocalName(localId, kvp.Key.Name, valStatement.ReadWrite);
                }
                catch (DuplicateSymbolException dse)
                {
                    Log.TerminateWithFatal(
                        dse.Message,
                        CurrentFileName,
                        EvilMessageCode.DuplicateSymbolInScope,
                        Line,
                        Column,
                        dse
                    );

                    // Dummy return to keep compiler happy.
                    return;
                }

                if (kvp.Value != null)
                {
                    Visit(kvp.Value);

                    Chunk.CodeGenerator.Emit(
                        OpCode.SETLOCAL,
                        sym.Id
                    );
                }
            }
        }
    }
}
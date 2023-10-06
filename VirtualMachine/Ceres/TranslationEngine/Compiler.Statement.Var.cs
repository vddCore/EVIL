using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
using Ceres.TranslationEngine.Scoping;
using EVIL.Grammar.AST.Statements;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(VarStatement varStatement)
        {
            foreach (var kvp in varStatement.Definitions)
            {
                Symbol sym;
                try
                {
                    var localId = Chunk.AllocateLocal();

                    sym = CurrentScope.DefineLocal(
                        kvp.Key.Name,
                        localId,
                        varStatement.ReadWrite,
                        varStatement.Line,
                        varStatement.Column
                    );

                    Chunk.DebugDatabase.SetLocalName(localId, kvp.Key.Name, varStatement.ReadWrite);
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
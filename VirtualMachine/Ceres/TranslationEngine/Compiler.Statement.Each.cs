using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
          public override void Visit(EachStatement eachStatement)
        {
            InNewLocalScopeDo(() =>
            {
                int keyLocal = Chunk.AllocateLocal();
                int valueLocal = -1;
                var isKeyValue = false;

                try
                {
                    CurrentScope.DefineLocal(
                        eachStatement.KeyIdentifier.Name,
                        keyLocal,
                        false,
                        eachStatement.KeyIdentifier.Line,
                        eachStatement.KeyIdentifier.Column
                    );
                }
                catch (DuplicateSymbolException dse)
                {
                    Log.TerminateWithFatal(
                        $"The symbol '{eachStatement.KeyIdentifier.Name}' already exists in this scope " +
                        $"and is a {dse.ExistingSymbol.TypeName} (previously defined on line {dse.Line}, column {dse.Column}).",
                        CurrentFileName,
                        EvilMessageCode.DuplicateSymbolInScope,
                        eachStatement.KeyIdentifier.Line,
                        eachStatement.KeyIdentifier.Column,
                        dse
                    );

                    // Return just in case - TerminateWithFatal should never return, ever.
                    return;
                }

                if (eachStatement.ValueIdentifier != null)
                {
                    valueLocal = Chunk.AllocateLocal();

                    try
                    {
                        CurrentScope.DefineLocal(
                            eachStatement.ValueIdentifier.Name,
                            valueLocal,
                            false,
                            eachStatement.ValueIdentifier.Line,
                            eachStatement.ValueIdentifier.Column
                        );
                    }
                    catch (DuplicateSymbolException dse)
                    {
                        Log.TerminateWithFatal(
                            $"The symbol '{eachStatement.KeyIdentifier.Name}' already exists in this scope " +
                            $"and is a {dse.ExistingSymbol.TypeName} (previously defined on line {dse.Line}, column {dse.Column}).",
                            CurrentFileName,
                            EvilMessageCode.DuplicateSymbolInScope,
                            eachStatement.ValueIdentifier.Line,
                            eachStatement.ValueIdentifier.Column,
                            dse
                        );

                        // Return just in case - TerminateWithFatal should never return, ever.
                        return;
                    }

                    isKeyValue = true;
                }

                Visit(eachStatement.Iterable);
                Chunk.CodeGenerator.Emit(OpCode.EACH);
                
                InNewLoopDo(Loop.LoopKind.Each, () =>
                {
                    Chunk.UpdateLabel(Loop.StartLabel, Chunk.CodeGenerator.IP);
                    Chunk.CodeGenerator.Emit(OpCode.NEXT, isKeyValue ? 1 : 0);
                    Chunk.CodeGenerator.Emit(OpCode.FJMP, Loop.EndLabel);
                    Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, keyLocal);

                    if (isKeyValue)
                    {
                        Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, valueLocal);
                    }

                    Visit(eachStatement.Statement);
                    Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.StartLabel);
                    Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
                    Chunk.CodeGenerator.Emit(OpCode.EEND);
                }, false);
            });
        }
    }
}
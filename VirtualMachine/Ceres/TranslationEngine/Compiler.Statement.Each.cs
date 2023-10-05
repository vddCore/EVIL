using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
          public override void Visit(EachStatement eachStatement)
        {
            InNewScopeDo(() =>
            {
                int keyLocal = Chunk.AllocateLocal();
                int valueLocal = -1;
                var isKeyValue = false;
                
                _currentScope.DefineLocal(
                    eachStatement.KeyIdentifier.Name,
                    keyLocal,
                    false,
                    eachStatement.KeyIdentifier.Line,
                    eachStatement.KeyIdentifier.Column
                );

                if (eachStatement.ValueIdentifier != null)
                {
                    valueLocal = Chunk.AllocateLocal();

                    _currentScope.DefineLocal(
                        eachStatement.ValueIdentifier.Name,
                        valueLocal,
                        false,
                        eachStatement.ValueIdentifier.Line,
                        eachStatement.ValueIdentifier.Column
                    );

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
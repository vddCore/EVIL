namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(TryStatement tryStatement)
    {
        var statementEndLabel = Chunk.CreateLabel();
        var enterInsnLabel = Chunk.CreateLabel();
            
        Chunk.CodeGenerator.Emit(OpCode.ENTER, Chunk.ProtectedBlocks.Count);
            
        _blockProtectors.Push(Chunk.AllocateBlockProtector());
        {
            BlockProtector.EnterLabelId = enterInsnLabel;
            BlockProtector.StartAddress = Chunk.CodeGenerator.IP;
            Visit(tryStatement.InnerStatement);
            BlockProtector.Length = Chunk.CodeGenerator.IP - BlockProtector.StartAddress;
            Chunk.CodeGenerator.Emit(OpCode.LEAVE);
            Chunk.CodeGenerator.Emit(OpCode.JUMP, statementEndLabel);
            BlockProtector.HandlerAddress = Chunk.CodeGenerator.IP;

            if (tryStatement.HandlerStatement != null)
            {
                InNewLocalScopeDo(() =>
                {
                    Chunk.CodeGenerator.Emit(OpCode.LEAVE);

                    if (tryStatement.HandlerExceptionLocal != null)
                    {
                        CurrentScope.DefineLocal(
                            tryStatement.HandlerExceptionLocal.Name,
                            Chunk.AllocateLocal(),
                            false,
                            tryStatement.HandlerExceptionLocal.Line,
                            tryStatement.HandlerExceptionLocal.Column
                        );

                        EmitVarSet(tryStatement.HandlerExceptionLocal.Name);
                    }
                    else
                    {
                        Chunk.CodeGenerator.Emit(OpCode.POP);
                    }

                    Visit(tryStatement.HandlerStatement);
                });
            }
            else
            {
                Chunk.CodeGenerator.Emit(OpCode.LEAVE);
                Chunk.CodeGenerator.Emit(OpCode.POP);
            }
        }
        _blockProtectors.Pop();
            
        Chunk.UpdateLabel(statementEndLabel, Chunk.CodeGenerator.IP);
    }
            
}
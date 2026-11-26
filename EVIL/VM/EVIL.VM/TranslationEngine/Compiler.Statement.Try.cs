namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(TryStatement tryStatement)
    {
        var statementEndLabel = Chunk.CreateLabel();
        var regionId = Chunk.ProtectedRegions.Count;
        var region = Chunk.AllocateProtectedRegion();

        Chunk.CodeGenerator.Emit(OpCode.ENTER, regionId);

        _blockProtectors.Push(regionId);
        {
            region.BodyStartAddress = Chunk.CodeGenerator.IP;
            Visit(tryStatement.InnerStatement);
            region.BodyEndAddress = Chunk.CodeGenerator.IP;
            if (!_isUnreachable)
            {
                Chunk.CodeGenerator.Emit(OpCode.LEAVE, regionId);
                Chunk.CodeGenerator.Emit(OpCode.JUMP, statementEndLabel);
            }
            region.HandlerAddress = Chunk.CodeGenerator.IP;

            if (tryStatement.HandlerStatement != null)
            {
                InNewLocalScopeDo(() =>
                {
                    _isUnreachable = false;
                    Chunk.CodeGenerator.Emit(OpCode.LEAVE_H, regionId);

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
                _isUnreachable = false;
                
                Chunk.CodeGenerator.Emit(OpCode.LEAVE_H, regionId);
                Chunk.CodeGenerator.Emit(OpCode.POP);
            }
        }
        _blockProtectors.Pop();
            
        Chunk.UpdateLabel(statementEndLabel, Chunk.CodeGenerator.IP);
    }
            
}
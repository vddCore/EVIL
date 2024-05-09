using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Statements;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(TryStatement tryStatement)
        {
            var statementEndLabel = Chunk.CreateLabel();
            
            Chunk.CodeGenerator.Emit(OpCode.ENTER, Chunk.ProtectedBlocks.Count);
            var protectionInfo = Chunk.AllocateBlockProtector();
            
            protectionInfo.StartAddress = Chunk.CodeGenerator.IP;
            Visit(tryStatement.InnerStatement);
            protectionInfo.Length = Chunk.CodeGenerator.IP - protectionInfo.StartAddress;
            Chunk.CodeGenerator.Emit(OpCode.LEAVE);
            Chunk.CodeGenerator.Emit(OpCode.JUMP, statementEndLabel);
            protectionInfo.HandlerAddress = Chunk.CodeGenerator.IP;
            
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

            Chunk.UpdateLabel(statementEndLabel, Chunk.CodeGenerator.IP);
        }
    }
}
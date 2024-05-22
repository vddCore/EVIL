using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ErrorExpression errorExpression)
        {
            if (errorExpression.UserDataTable == null && errorExpression.ImplicitMessageConstant == null)
            {
                Log.TerminateWithFatal(
                    "An error expression must have at least an implicit message constant.",
                    CurrentFileName,
                    EvilMessageCode.MissingErrorInformation,
                    errorExpression.Line,
                    errorExpression.Column
                );
                
                return;
            }
            
            if (errorExpression.UserDataTable != null)
            {
                Visit(errorExpression.UserDataTable);
            }
            else
            {
                Chunk.CodeGenerator.Emit(OpCode.TABNEW);
            }

            if (errorExpression.ImplicitMessageConstant != null)
            {
                Visit(errorExpression.ImplicitMessageConstant!);
                Chunk.CodeGenerator.Emit(
                    OpCode.LDSTR, 
                    (int)Chunk.StringPool.FetchOrAdd("msg")
                );
                Chunk.CodeGenerator.Emit(OpCode.ELINIT);
            }

            Chunk.CodeGenerator.Emit(OpCode.ERRNEW);
        }
    }
}
namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.CommonTypes.Diagnostics;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(ErrorExpression errorExpression)
    {
        if (errorExpression.UserDataTable == null && errorExpression.ImplicitMessageConstant == null)
        {
            Log.TerminateWithFatal(
                CompilerDiagnostics.MissingErrorInformation.Create(
                    GetLocation(errorExpression)
                )
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
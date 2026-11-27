namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.VM.ExecutionEngine.Diagnostics;

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
            Visit(errorExpression.ImplicitMessageConstant);
            Chunk.CodeGenerator.Emit(
                OpCode.LDSTR, 
                (int)Chunk.StringPool.FetchOrAdd("msg")
            );
            Chunk.CodeGenerator.Emit(OpCode.ELINIT);
        }

        Chunk.CodeGenerator.Emit(OpCode.ERRNEW);
    }
}
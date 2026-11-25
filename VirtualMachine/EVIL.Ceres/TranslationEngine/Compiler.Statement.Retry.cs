namespace EVIL.Ceres.TranslationEngine;

using EVIL.Diagnostics;
using ExecutionEngine.Diagnostics;
using Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(RetryStatement retryStatement)
    {
        if (_blockProtectors.Count == 0)
        {
            Log.TerminateWithFatal(
                CompilerDiagnostics.UnexpectedSyntaxNode.Create(
                    GetLocation(retryStatement),
                    "retry"
                )
            );
        }
        
        Chunk.CodeGenerator.Emit(
            OpCode.RETRY,
            BlockProtector
        );
        
        _isUnreachable = true;
    }
}
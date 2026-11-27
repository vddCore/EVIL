namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Statements;
using EVIL.VM.ExecutionEngine.Diagnostics;

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
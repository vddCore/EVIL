namespace EVIL.Ceres.TranslationEngine;

using System.Linq;
using Diagnostics;
using ExecutionEngine.Diagnostics;
using Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(RetryStatement retryStatement)
    {
        if (_blockProtectors.Count == 0)
        {
            Log.TerminateWithFatal(
                "`retry' was not expected at this time.",
                CurrentFileName,
                EvilMessageCode.UnexpectedSyntaxNodeFound,
                retryStatement.Line,
                retryStatement.Column
            );
        }
        
        Chunk.CodeGenerator.Emit(
            OpCode.JUMP, 
            BlockProtector.EnterLabelId
        );
    }
}
namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.VM.ExecutionEngine.Diagnostics;

public partial class Compiler
{
    protected override void Visit(UnaryExpression unaryExpression)
    {
        var reduced = unaryExpression.Reduce();
        if (reduced != unaryExpression)
        {
            if (reduced is UnaryExpression reducedUnary)
            {
                unaryExpression = reducedUnary;
            }
            else
            {
                Visit(reduced);
                return;
            }
        }

        Visit(unaryExpression.Right);

        if (unaryExpression.Type != UnaryOperationType.Plus)
        {
            Chunk.CodeGenerator.Emit(
                unaryExpression.Type switch
                {
                    UnaryOperationType.Minus => OpCode.ANEG,
                    UnaryOperationType.Length => OpCode.LENGTH,
                    UnaryOperationType.BitwiseNot => OpCode.BNOT,
                    UnaryOperationType.LogicalNot => OpCode.LNOT,
                    UnaryOperationType.ToString => OpCode.TOSTRING,
                    UnaryOperationType.ToNumber => OpCode.TONUMBER,
                    _ => Log.TerminateWithInternalFailure(
                        CompilerDiagnostics.InternalCompilerError.Create(
                            GetLocation(Line, Column),
                            $"Invalid unary operation type '{unaryExpression.Type}'."
                        ),
                        dummyReturn: OpCode.NOOP
                    )
                }
            );
        }
    }
}
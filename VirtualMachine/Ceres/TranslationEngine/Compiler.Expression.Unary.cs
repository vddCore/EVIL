using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(UnaryExpression unaryExpression)
        {
            Visit(unaryExpression.Right);

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
                        $"Invalid unary operation type '{unaryExpression.Type}'.",
                        CurrentFileName,
                        line: Line,
                        column: Column,
                        dummyReturn: OpCode.NOOP
                    )
                }
            );
        }
    }
}
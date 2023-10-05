using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(BinaryExpression binaryExpression)
        {
            Visit(binaryExpression.Left);
            Visit(binaryExpression.Right);

            Chunk.CodeGenerator.Emit(
                binaryExpression.Type switch
                {
                    BinaryOperationType.Add => OpCode.ADD,
                    BinaryOperationType.Subtract => OpCode.SUB,
                    BinaryOperationType.Divide => OpCode.DIV,
                    BinaryOperationType.Modulo => OpCode.MOD,
                    BinaryOperationType.Multiply => OpCode.MUL,
                    BinaryOperationType.BitwiseAnd => OpCode.BAND,
                    BinaryOperationType.BitwiseOr => OpCode.BOR,
                    BinaryOperationType.BitwiseXor => OpCode.BXOR,
                    BinaryOperationType.ShiftLeft => OpCode.SHL,
                    BinaryOperationType.ShiftRight => OpCode.SHR,
                    BinaryOperationType.ExistsIn => OpCode.EXISTS,
                    BinaryOperationType.LogicalAnd => OpCode.LAND,
                    BinaryOperationType.LogicalOr => OpCode.LOR,
                    BinaryOperationType.DeepEqual => OpCode.DEQ,
                    BinaryOperationType.DeepNotEqual => OpCode.DNE,
                    BinaryOperationType.Equal => OpCode.CEQ,
                    BinaryOperationType.NotEqual => OpCode.CNE,
                    BinaryOperationType.Greater => OpCode.CGT,
                    BinaryOperationType.Less => OpCode.CLT,
                    BinaryOperationType.GreaterOrEqual => OpCode.CGE,
                    BinaryOperationType.LessOrEqual => OpCode.CLE,
                    _ => Log.TerminateWithInternalFailure(
                        $"Invalid binary operation type '{binaryExpression.Type}'.",
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
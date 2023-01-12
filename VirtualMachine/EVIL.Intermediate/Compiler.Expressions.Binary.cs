using EVIL.Grammar;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(BinaryExpression binaryExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            Visit(binaryExpression.Left);
            Visit(binaryExpression.Right);

            switch (binaryExpression.Type)
            {
                case BinaryOperationType.Plus:
                    cg.Emit(OpCode.ADD);
                    break;

                case BinaryOperationType.Minus:
                    cg.Emit(OpCode.SUB);
                    break;

                case BinaryOperationType.Divide:
                    cg.Emit(OpCode.DIV);
                    break;

                case BinaryOperationType.Multiply:
                    cg.Emit(OpCode.MUL);
                    break;

                case BinaryOperationType.Modulo:
                    cg.Emit(OpCode.MOD);
                    break;

                case BinaryOperationType.BitwiseAnd:
                    cg.Emit(OpCode.AND);
                    break;

                case BinaryOperationType.BitwiseOr:
                    cg.Emit(OpCode.OR);
                    break;

                case BinaryOperationType.BitwiseXor:
                    cg.Emit(OpCode.XOR);
                    break;

                case BinaryOperationType.Less:
                    cg.Emit(OpCode.CLT);
                    break;

                case BinaryOperationType.Greater:
                    cg.Emit(OpCode.CGT);
                    break;

                case BinaryOperationType.LessOrEqual:
                    cg.Emit(OpCode.CLE);
                    break;

                case BinaryOperationType.GreaterOrEqual:
                    cg.Emit(OpCode.CGE);
                    break;

                case BinaryOperationType.Equal:
                    cg.Emit(OpCode.CEQ);
                    break;

                case BinaryOperationType.NotEqual:
                    cg.Emit(OpCode.CNE);
                    break;

                case BinaryOperationType.LogicalOr:
                    cg.Emit(OpCode.LOR);
                    break;

                case BinaryOperationType.LogicalAnd:
                    cg.Emit(OpCode.LAND);
                    break;

                case BinaryOperationType.ShiftRight:
                    cg.Emit(OpCode.SHR);
                    break;

                case BinaryOperationType.ShiftLeft:
                    cg.Emit(OpCode.SHL);
                    break;

                case BinaryOperationType.ExistsIn:
                    cg.Emit(OpCode.XINT);
                    break;

                default:
                    throw new CompilerException(
                        $"Unrecognized binary operation type '{binaryExpression.Type}'",
                        binaryExpression.Line
                    );
            }
        }
    }
}
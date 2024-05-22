using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar;
using EVIL.Grammar.AST.Expressions;

namespace EVIL.Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(BinaryExpression binaryExpression)
        {
            Visit(binaryExpression.Left);
            Visit(binaryExpression.Right);

            switch (binaryExpression.Type)
            {
                case BinaryOperationType.Add:
                {
                    Chunk.CodeGenerator.Emit(OpCode.ADD);
                    break;
                }

                case BinaryOperationType.Subtract:
                {
                    Chunk.CodeGenerator.Emit(OpCode.SUB);
                    break;
                }

                case BinaryOperationType.Divide:
                {
                    Chunk.CodeGenerator.Emit(OpCode.DIV);
                    break;
                }

                case BinaryOperationType.Modulo:
                {
                    Chunk.CodeGenerator.Emit(OpCode.MOD);
                    break;
                }

                case BinaryOperationType.Multiply:
                {
                    Chunk.CodeGenerator.Emit(OpCode.MUL);
                    break;
                }

                case BinaryOperationType.BitwiseAnd:
                {
                    Chunk.CodeGenerator.Emit(OpCode.BAND);
                    break;
                }

                case BinaryOperationType.BitwiseOr:
                {
                    Chunk.CodeGenerator.Emit(OpCode.BOR);
                    break;
                }

                case BinaryOperationType.BitwiseXor:
                {
                    Chunk.CodeGenerator.Emit(OpCode.BXOR);
                    break;
                }

                case BinaryOperationType.ShiftLeft:
                {
                    Chunk.CodeGenerator.Emit(OpCode.SHL);
                    break;
                }

                case BinaryOperationType.ShiftRight:
                {
                    Chunk.CodeGenerator.Emit(OpCode.SHR);
                    break;
                }

                case BinaryOperationType.ExistsIn:
                {
                    Chunk.CodeGenerator.Emit(OpCode.EXISTS);
                    break;
                }

                case BinaryOperationType.DoesNotExistIn:
                {
                    Chunk.CodeGenerator.Emit(OpCode.EXISTS);
                    Chunk.CodeGenerator.Emit(OpCode.LNOT);

                    break;
                }

                case BinaryOperationType.LogicalAnd:
                {
                    Chunk.CodeGenerator.Emit(OpCode.LAND);
                    break;
                }

                case BinaryOperationType.LogicalOr:
                {
                    Chunk.CodeGenerator.Emit(OpCode.LOR);
                    break;
                }

                case BinaryOperationType.DeepEqual:
                {
                    Chunk.CodeGenerator.Emit(OpCode.DEQ);
                    break;
                }

                case BinaryOperationType.DeepNotEqual:
                {
                    Chunk.CodeGenerator.Emit(OpCode.DNE);
                    break;
                }

                case BinaryOperationType.Equal:
                {
                    Chunk.CodeGenerator.Emit(OpCode.CEQ);
                    break;
                }

                case BinaryOperationType.NotEqual:
                {
                    Chunk.CodeGenerator.Emit(OpCode.CNE);
                    break;
                }

                case BinaryOperationType.Greater:
                {
                    Chunk.CodeGenerator.Emit(OpCode.CGT);
                    break;
                }

                case BinaryOperationType.Less:
                {
                    Chunk.CodeGenerator.Emit(OpCode.CLT);
                    break;
                }

                case BinaryOperationType.GreaterOrEqual:
                {
                    Chunk.CodeGenerator.Emit(OpCode.CGE);
                    break;
                }

                case BinaryOperationType.LessOrEqual:
                {
                    Chunk.CodeGenerator.Emit(OpCode.CLE);
                    break;
                }

                default:
                {
                    Log.TerminateWithInternalFailure(
                        $"Invalid binary operation type '{binaryExpression.Type}'.",
                        CurrentFileName,
                        line: Line,
                        column: Column,
                        dummyReturn: OpCode.NOOP
                    );

                    break;
                }
            }
        }
    }
}
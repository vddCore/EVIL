namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.CommonTypes.TypeSystem;
using EVIL.Grammar;
using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(BinaryExpression binaryExpression)
    {
        if (binaryExpression.Type == BinaryOperationType.LogicalAnd)
        {
            EmitLogicalAnd(binaryExpression);
            return;
        }

        if (binaryExpression.Type == BinaryOperationType.LogicalOr)
        {
            EmitLogicalOr(binaryExpression);
            return;
        }

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

    private void EmitLogicalAnd(BinaryExpression binaryExpression)
    {
        var falseLabel = Chunk.CreateLabel();
        var evalRightLabel = Chunk.CreateLabel();
        var endLabel = Chunk.CreateLabel();

        Visit(binaryExpression.Left);
        
        // if it's a table then can't afford to short circuit - __land exists.
        Chunk.CodeGenerator.Emit(OpCode.DUP);
        Chunk.CodeGenerator.Emit(OpCode.TYPE);
        Chunk.CodeGenerator.Emit(OpCode.LDTYPE, (byte)DynamicValueType.Table);
        Chunk.CodeGenerator.Emit(OpCode.CEQ);
        Chunk.CodeGenerator.Emit(OpCode.TJMP, evalRightLabel);

        Chunk.CodeGenerator.Emit(OpCode.DUP);
        Chunk.CodeGenerator.Emit(OpCode.FJMP, falseLabel);

        Chunk.UpdateLabel(evalRightLabel, Chunk.CodeGenerator.IP);
        Visit(binaryExpression.Right);
        Chunk.CodeGenerator.Emit(OpCode.LAND);
        Chunk.CodeGenerator.Emit(OpCode.JUMP, endLabel);

        Chunk.UpdateLabel(falseLabel, Chunk.CodeGenerator.IP);
        Chunk.CodeGenerator.Emit(OpCode.POP);
        Chunk.CodeGenerator.Emit(OpCode.LDFALSE);

        Chunk.UpdateLabel(endLabel, Chunk.CodeGenerator.IP);
    }

    private void EmitLogicalOr(BinaryExpression binaryExpression)
    {
        var trueLabel = Chunk.CreateLabel();
        var evalRightLabel = Chunk.CreateLabel();
        var endLabel = Chunk.CreateLabel();

        Visit(binaryExpression.Left);

        // same as above but __lor exists.
        Chunk.CodeGenerator.Emit(OpCode.DUP);
        Chunk.CodeGenerator.Emit(OpCode.TYPE);
        Chunk.CodeGenerator.Emit(OpCode.LDTYPE, (byte)DynamicValueType.Table);
        Chunk.CodeGenerator.Emit(OpCode.CEQ);
        Chunk.CodeGenerator.Emit(OpCode.TJMP, evalRightLabel);

        Chunk.CodeGenerator.Emit(OpCode.DUP);
        Chunk.CodeGenerator.Emit(OpCode.TJMP, trueLabel);

        Chunk.UpdateLabel(evalRightLabel, Chunk.CodeGenerator.IP);
        Visit(binaryExpression.Right);
        Chunk.CodeGenerator.Emit(OpCode.LOR);
        Chunk.CodeGenerator.Emit(OpCode.JUMP, endLabel);

        Chunk.UpdateLabel(trueLabel, Chunk.CodeGenerator.IP);
        Chunk.CodeGenerator.Emit(OpCode.POP);
        Chunk.CodeGenerator.Emit(OpCode.LDTRUE);

        Chunk.UpdateLabel(endLabel, Chunk.CodeGenerator.IP);
    }
}
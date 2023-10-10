using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(UnaryExpression unaryExpression)
        {
            var operand = unaryExpression.Right;

            if (operand is NumberConstant numberConstant && unaryExpression.Type == UnaryOperationType.Minus)
            {
                Chunk.CodeGenerator.Emit(OpCode.LDNUM, -numberConstant.Value);
            }
            else
            {
                Visit(operand);

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
    }
}
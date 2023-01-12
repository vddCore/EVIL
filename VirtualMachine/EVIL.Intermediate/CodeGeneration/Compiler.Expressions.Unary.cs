using EVIL.Grammar;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(UnaryExpression unaryExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            Visit(unaryExpression.Right);

            switch (unaryExpression.Type)
            {
                case UnaryOperationType.Minus:
                    cg.Emit(OpCode.UNM);
                    break;

                case UnaryOperationType.Length:
                    cg.Emit(OpCode.LEN);
                    break;

                case UnaryOperationType.BitwiseNot:
                    cg.Emit(OpCode.NOT);
                    break;

                case UnaryOperationType.Plus:
                    break;

                case UnaryOperationType.Negation:
                    cg.Emit(OpCode.LNOT);
                    break;

                case UnaryOperationType.ToString:
                    cg.Emit(OpCode.TOSTR);
                    break;

                case UnaryOperationType.ToNumber:
                    cg.Emit(OpCode.TONUM);
                    break;

                default:
                    throw new CompilerException(
                        $"Unrecognized unary operation type '{unaryExpression.Type}'",
                        unaryExpression.Line,
                        unaryExpression.Column
                    );
            }
        }
    }
}
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(TableExpression tableExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            cg.Emit(OpCode.NEWTB);

            if (tableExpression.Initializers.Count > 0)
            {
                if (tableExpression.Keyed)
                {
                    foreach (var expr in tableExpression.Initializers)
                    {
                        if (expr is not KeyValuePairExpression kvp)
                        {
                            throw new CompilerException(
                                $"Invalid table initializer.",
                                expr.Line,
                                expr.Column
                            );
                        }

                        cg.Emit(OpCode.DUP);
                        Visit(kvp.KeyNode);
                        Visit(kvp.ValueNode);
                        cg.Emit(OpCode.STE, 0);
                    }
                }
                else
                {
                    for (var i = 0; i < tableExpression.Initializers.Count; i++)
                    {
                        cg.Emit(OpCode.DUP);
                        EmitConstantLoad(cg, i);
                        Visit(tableExpression.Initializers[i]);
                        cg.Emit(OpCode.STE, 0);
                    }
                }
            }
        }
    }
}
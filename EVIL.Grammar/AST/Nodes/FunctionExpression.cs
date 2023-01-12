using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class FunctionExpression : Expression
    {
        public List<string> Parameters { get; }
        public BlockStatement Statements { get; }

        public FunctionExpression(List<string> parameters, BlockStatement statements)
        {
            Parameters = parameters;
            Statements = statements;

            Reparent(Statements);
        }
    }
}
using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class FunctionDefinitionAnonymousNode : Expression
    {
        public List<string> Parameters { get; }
        public BlockStatement Statements { get; }

        public FunctionDefinitionAnonymousNode(List<string> parameters, BlockStatement statements)
        {
            Parameters = parameters;
            Statements = statements;

            Reparent(Statements);
        }
    }
}
using System.Collections.Generic;

namespace EVIL.Grammar.AST.Statements
{
    public sealed class FunctionDefinition : Statement
    {
        public string Identifier { get; }

        public List<string> Parameters { get; }
        public BlockStatement Statements { get; }

        public FunctionDefinition(string identifier, List<string> parameters, BlockStatement statements)
        {
            Identifier = identifier;

            Parameters = parameters;
            Statements = statements;

            Reparent(Statements);
        }
    }
}
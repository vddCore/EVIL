using System.Collections.Generic;

namespace EVIL.Grammar.AST.Statements
{
    public sealed class FunctionDefinition : Statement
    {
        public string Identifier { get; }

        public List<string> Parameters { get; }
        public Statement Statement { get; }

        public FunctionDefinition(string identifier, List<string> parameters, Statement statement)
        {
            Identifier = identifier;

            Parameters = parameters;
            Statement = statement;

            Reparent(Statement);
        }
    }
}
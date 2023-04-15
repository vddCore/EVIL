using System.Collections.Generic;

namespace EVIL.Grammar.AST
{
    public sealed class Program : AstNode
    {
        public List<Statement> Statements { get; } 

        public Program(List<Statement> statements)
        {
            Statements = statements;

            for (var i = 0; i < Statements.Count; i++)
            {
                Reparent(Statements[i]);
            }
        }
    }
}
using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class IfStatement : Statement
    {
        public List<Expression> Conditions { get; } = new();
        public List<Statement> Statements { get; } = new();
        
        public Statement ElseBranch { get; internal set; }
    }
}

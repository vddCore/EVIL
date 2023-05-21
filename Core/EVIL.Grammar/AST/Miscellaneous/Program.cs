using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Miscellaneous
{
    public sealed class Program : AstNode
    {
        public List<TopLevelStatement> Statements { get; } 

        public Program(List<TopLevelStatement> statements)
        {
            Statements = statements;
            Reparent(Statements);
        }
    }
}
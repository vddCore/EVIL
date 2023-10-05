using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Miscellaneous
{
    public sealed class ProgramNode : AstNode
    {
        public List<TopLevelStatement> Statements { get; } 

        public ProgramNode(List<TopLevelStatement> statements)
        {
            Statements = statements;
            Reparent(Statements);
        }
    }
}
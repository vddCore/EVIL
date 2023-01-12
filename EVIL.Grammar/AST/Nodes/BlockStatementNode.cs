using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class BlockStatementNode : AstNode
    {
        public List<AstNode> Statements { get; }

        public BlockStatementNode(List<AstNode> statements)
        {
            Statements = statements;

            for (var i = 0; i < statements.Count; i++)
            {
                Reparent(statements[i]);
            }
        }
    }
}
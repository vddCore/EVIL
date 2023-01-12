using System.Diagnostics.CodeAnalysis;

namespace EVIL.Grammar.AST.Nodes
{
    public class ReturnNode : AstNode
    {
        [MaybeNull]
        public AstNode Right { get; }

        public ReturnNode(AstNode right)
        {
            Right = right;

            if (Right != null)
            {
                Reparent(Right);
            }
        }
    }
}

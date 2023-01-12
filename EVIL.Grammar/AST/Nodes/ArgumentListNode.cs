using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class ArgumentListNode : AstNode
    {
        public List<AstNode> Arguments { get; }

        public ArgumentListNode(List<AstNode> arguments)
        {
            Arguments = arguments;

            Reparent(arguments.ToArray());
        }
    }
}
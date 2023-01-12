using System.Collections.Generic;
using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class IndexingNode : AstNode
    {
        public AstNode Indexable { get; }
        public Queue<AstNode> KeyExpressions { get; }

        public bool WillBeAssigned { get; }

        public IndexingNode(AstNode indexable, Queue<AstNode> keyExpressions, bool willBeAssigned)
        {
            Indexable = indexable;
            KeyExpressions = keyExpressions;

            WillBeAssigned = willBeAssigned;
        }
    }
}


using System.Collections.Generic;
using System.Linq;

namespace EVIL.Grammar.AST
{
    public abstract class AstNode
    {
        public int Line { get; set; }
        public int Column { get; set; }
        
        public AstNode? Parent { get; set; }

        public bool IsConstant => this is ConstantExpression;

        protected void Reparent(params AstNode[] nodes)
        {
            for (var i = 0; i < nodes.Length; i++)
                nodes[i].Parent = this;
        }

        protected void Reparent(IEnumerable<AstNode> nodes) 
            => Reparent(nodes.ToArray());
    }
}
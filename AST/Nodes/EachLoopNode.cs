using System.Collections.Generic;
using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class EachLoopNode : AstNode
    {
        public VariableNode KeyNode { get; }
        public VariableNode ValueNode { get; }
        public AstNode TableNode { get; }
        public List<AstNode> StatementList { get; }

        public EachLoopNode(VariableNode keyNode, VariableNode valueNode, AstNode tableNode, List<AstNode> statementList)
        {
            KeyNode = keyNode;
            ValueNode = valueNode;
            TableNode = tableNode;

            StatementList = statementList;
        }
    }
}

using System.Collections.Generic;
using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class FunctionCallNode : AstNode
    {
        public string MemberName { get; }
        public List<AstNode> Parameters { get; }

        public FunctionCallNode(string memberName, List<AstNode> parameters)
        {
            MemberName = memberName;
            Parameters = parameters;
        }
    }
}
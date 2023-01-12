using System.Collections.Generic;
using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class RootNode : AstNode
    {
        public List<AstNode> Children { get; }

        public RootNode()
        {
            Children = new List<AstNode>();
        }

        public FunctionDefinitionNode FindChildFunctionDefinition(string fnName)
        {
            for (var i = 0; i < Children.Count; i++)
            {
                if (Children[i] is FunctionDefinitionNode fdn)
                {
                    if (fdn.Name == fnName)
                    {
                        return fdn;
                    }
                }
            }

            return null;
        }
    }
}
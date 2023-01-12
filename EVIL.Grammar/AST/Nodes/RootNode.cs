using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
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
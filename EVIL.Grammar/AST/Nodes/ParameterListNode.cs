using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class ParameterListNode : AstNode
    {
        public List<string> Identifiers { get; }

        public ParameterListNode(List<string> identifiers)
        {
            Identifiers = identifiers;
        }
    }
}
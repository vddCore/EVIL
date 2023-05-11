using System.Collections.Generic;

namespace EVIL.Grammar.AST
{
    public sealed class ParameterList : AstNode
    {
        public List<ParameterNode> Parameters { get; }

        public ParameterList(List<ParameterNode> parameters)
        {
            Parameters = parameters;
            Reparent(Parameters);
        }
    }
}
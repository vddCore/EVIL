using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Miscellaneous
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
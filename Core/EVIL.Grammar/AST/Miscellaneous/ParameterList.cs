namespace EVIL.Grammar.AST.Miscellaneous;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

public sealed class ParameterList : AstNode
{
    public List<ParameterNode> Parameters { get; }

    public ParameterList(List<ParameterNode> parameters)
    {
        Parameters = parameters;
        Reparent(Parameters);
    }
}
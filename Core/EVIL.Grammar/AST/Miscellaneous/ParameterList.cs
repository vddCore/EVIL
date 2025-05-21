namespace EVIL.Grammar.AST.Miscellaneous;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

public sealed class ParameterList : AstNode
{
    public List<ParameterNode> Parameters { get; }
    public bool HasSelf { get; }

    public ParameterList(List<ParameterNode> parameters, bool hasSelf)
    {
        Parameters = parameters;
        HasSelf = hasSelf;
        
        Reparent(Parameters);
    }
}
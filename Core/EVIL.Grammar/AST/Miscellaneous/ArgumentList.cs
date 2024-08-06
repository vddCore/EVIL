namespace EVIL.Grammar.AST.Miscellaneous;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

public sealed class ArgumentList : AstNode
{
    public List<Expression> Arguments { get; }
    public bool IsVariadic { get; }

    public ArgumentList(List<Expression> arguments, bool isVariadic)
    {
        Arguments = arguments;
        IsVariadic = isVariadic;
            
        Reparent(Arguments);
    }
}
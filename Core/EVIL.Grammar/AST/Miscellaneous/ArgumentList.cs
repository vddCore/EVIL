using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Miscellaneous
{
    public class ArgumentList : AstNode
    {
        public List<Expression> Arguments { get; }

        public ArgumentList(List<Expression> arguments)
        {
            Arguments = arguments;
            Reparent(Arguments);
        }
    }
}
using System.Collections.Generic;

namespace EVIL.Grammar.AST
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
using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Miscellaneous
{
    public class ArgumentList : AstNode
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
}
using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private FunctionCallExpression FunctionCall(Expression callee)
        {
            var (line, col) = (_lexer.State.Line, _lexer.State.Column);

            if (callee is NilConstant)
            {
                throw new ParserException(
                    "'nil' is not a valid invocation target.",
                    (callee.Line, callee.Column)
                );
            }
            
            return new FunctionCallExpression(callee, ArgumentList()) 
                { Line = line, Column = col };
        }
    }
}

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private InvocationExpression InvocationExpression(Expression callee)
        {
            var (line, col) = (_lexer.State.Line, _lexer.State.Column);

            if (callee is NilConstant)
            {
                throw new ParserException(
                    "'nil' is not a valid invocation target.",
                    (callee.Line, callee.Column)
                );
            }

            var argumentList = ArgumentList();            
            var expr = new InvocationExpression(callee, argumentList) 
                { Line = line, Column = col };

            return expr;
        }
    }
}

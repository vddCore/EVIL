namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Constants;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Lexical;

public partial class Parser
{
    private InvocationExpression InvocationExpression(Expression callee)
    {
        var (line, col) = (_lexer.State.Line, _lexer.State.Column);
        var isConditional = CurrentToken.Type == TokenType.ElvisInvocation;

        if (callee is NilConstant)
        {
            throw new ParserException(
                "'nil' is not a valid invocation target.",
                (callee.Line, callee.Column)
            );
        }

        var argumentList = ArgumentList();            
        var expr = new InvocationExpression(callee, argumentList, isConditional) 
            { Line = line, Column = col };

        return expr;
    }
}

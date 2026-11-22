namespace EVIL.Grammar;

using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private YieldExpression YieldExpression()
    {
        var (line, col) = Match(Token.Yield);

        var invocation = PostfixExpression();

        if (invocation is not (AST.Expressions.InvocationExpression or AST.Expressions.SelfInvocationExpression))
        {
            throw new ParserException(
                $"Expected function invocation after 'yield', found '{invocation.GetType().Name}'.",
                (invocation.Line, invocation.Column)
            );
        }

        return new YieldExpression(invocation)
            { Line = line, Column = col };
    }
}
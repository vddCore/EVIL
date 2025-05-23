namespace EVIL.Grammar;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Statements;

public partial class Parser
{
    private static ExpressionStatement ExpressionStatement(Expression expression)
    {
        if (!expression.IsValidExpressionStatement)
        {
            throw new ParserException(
                "Only assignment, invocation, increment, decrement and yield expressions can be used as statements.",
                (expression.Line, expression.Column)
            );
        }
            
        var node = new ExpressionStatement(expression);
        return node;
    }
}
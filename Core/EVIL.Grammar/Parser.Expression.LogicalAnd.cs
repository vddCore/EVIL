namespace EVIL.Grammar;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private Expression LogicalAndExpression()
    {
        var node = InclusiveOrExpression();
        var token = CurrentToken;

        while (token.Type == TokenType.LogicalAnd)
        {
            var (line, col) = Match(Token.LogicalAnd);
                
            node = new BinaryExpression(node, InclusiveOrExpression(), BinaryOperationType.LogicalAnd)
                { Line = line, Column = col };

            token = CurrentToken;
        }

        return node;
    }
}
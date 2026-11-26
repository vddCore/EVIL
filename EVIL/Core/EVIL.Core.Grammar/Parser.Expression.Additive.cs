namespace EVIL.Core.Grammar;

using System.Linq;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Lexical;

public partial class Parser
{
    private static readonly TokenType[] _additiveOperators =
    [
        TokenType.Plus,
        TokenType.Minus
    ];

    private Expression AdditiveExpression()
    {
        return AdditiveExpression(MultiplicativeExpression());
    }

    private Expression AdditiveExpression(Expression node)
    {
        var token = CurrentToken;

        while (_additiveOperators.Contains(token.Type))
        {
            if (token.Type == TokenType.Plus)
            {
                var (line, col) = Match(Token.Plus);
                node = new BinaryExpression(node, MultiplicativeExpression(), BinaryOperationType.Add)
                    { Line = line, Column = col };
            }
            else if (token.Type == TokenType.Minus)
            {
                var (line, col) = Match(Token.Minus);
                node = new BinaryExpression(node, MultiplicativeExpression(), BinaryOperationType.Subtract)
                    { Line = line, Column = col };
            }

            token = CurrentToken;
        }

        return node;
    }
}
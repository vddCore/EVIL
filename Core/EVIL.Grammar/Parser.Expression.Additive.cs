namespace EVIL.Grammar;

using System.Linq;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private static readonly TokenType[] _additiveOperators = new[]
    {
        TokenType.Plus,
        TokenType.Minus,
    };

    private Expression AdditiveExpression()
    {
        var node = MultiplicativeExpression();
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
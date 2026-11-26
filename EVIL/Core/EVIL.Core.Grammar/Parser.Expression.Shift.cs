namespace EVIL.Core.Grammar;

using System.Collections.Generic;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Lexical;

public partial class Parser
{
    private readonly List<TokenType> _shiftOperators =
    [
        TokenType.ShiftLeft,
        TokenType.ShiftRight
    ];

    private Expression ShiftExpression()
    {
        return ShiftExpressionCore(
            AdditiveExpression()
        );
    }

    private Expression ShiftExpressionFrom(Expression firstTerm)
    {
        return ShiftExpressionCore(
            AdditiveExpression(
                MultiplicativeExpression(
                    firstTerm
                )
            )
        );
    }

    private Expression ShiftExpressionCore(Expression node)
    {
        var token = CurrentToken;

        while (_shiftOperators.Contains(token.Type))
        {
            if (token.Type == TokenType.ShiftLeft)
            {
                var (line, col) = Match(Token.ShiftLeft);

                node = new BinaryExpression(node, AdditiveExpression(), BinaryOperationType.ShiftLeft)
                    { Line = line, Column = col };
            }
            else if (token.Type == TokenType.ShiftRight)
            {
                var (line, col) = Match(Token.ShiftRight);
                
                node = new BinaryExpression(node, AdditiveExpression(), BinaryOperationType.ShiftRight)
                    { Line = line, Column = col };
            }

            token = CurrentToken;
        }

        return node;
    }
}
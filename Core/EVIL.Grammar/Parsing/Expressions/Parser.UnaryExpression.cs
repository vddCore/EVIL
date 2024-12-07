namespace EVIL.Grammar.Parsing;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private List<TokenType> _unaryOperators = new()
    {
        TokenType.Plus,
        TokenType.Minus,
        TokenType.LogicalNot,
        TokenType.BitwiseNot,
        TokenType.Length,
        TokenType.AsNumber,
        TokenType.AsString
    };

    private Expression UnaryExpression()
    {
        var token = CurrentToken;

        if (token.Type == TokenType.AsString)
        {
            var (line, col) = Match(Token.AsString);

            return new UnaryExpression(UnaryExpression(), UnaryOperationType.ToString)
                { Line = line, Column = col };
        }
        else if (token.Type == TokenType.Plus)
        {
            var (line, col) = Match(Token.Plus);

            return new UnaryExpression(UnaryExpression(), UnaryOperationType.Plus)
                { Line = line, Column = col };
        }
        else if (token.Type == TokenType.Minus)
        {
            var (line, col) = Match(Token.Minus);

            return new UnaryExpression(UnaryExpression(), UnaryOperationType.Minus)
                { Line = line, Column = col };
        }
        else if (token.Type == TokenType.LogicalNot)
        {
            var (line, col) = Match(Token.LogicalNot);

            return new UnaryExpression(UnaryExpression(), UnaryOperationType.LogicalNot)
                { Line = line, Column = col };
        }
        else if (token.Type == TokenType.BitwiseNot)
        {
            var (line, col) = Match(Token.BitwiseNot);

            return new UnaryExpression(UnaryExpression(), UnaryOperationType.BitwiseNot)
                { Line = line, Column = col };
        }
        else if (token.Type == TokenType.AsNumber)
        {
            var (line, col) = Match(Token.AsNumber);

            return new UnaryExpression(UnaryExpression(), UnaryOperationType.ToNumber)
                { Line = line, Column = col };
        }
        else if (token.Type == TokenType.Length)
        {
            var (line, col) = Match(Token.Length);
                
            return new UnaryExpression(UnaryExpression(), UnaryOperationType.Length) 
                { Line = line, Column = col };
        }

        return PostfixExpression();
    }
}
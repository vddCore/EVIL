﻿namespace EVIL.Grammar.Parsing;

using System.Linq;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private static readonly TokenType[] _multiplicativeOperators = new[]
    {
        TokenType.Multiply,
        TokenType.Divide,
        TokenType.Modulo
    };

    private Expression MultiplicativeExpression()
    {
        var node = PatternExpression();
        var token = CurrentToken;

        while (_multiplicativeOperators.Contains(token.Type))
        {
            if (token.Type == TokenType.Multiply)
            {
                var (line, col) = Match(Token.Multiply);

                node = new BinaryExpression(node, PatternExpression(), BinaryOperationType.Multiply)
                    { Line = line, Column = col };
            }
            else if (token.Type == TokenType.Divide)
            {
                var (line, col) = Match(Token.Divide);

                node = new BinaryExpression(node, PatternExpression(), BinaryOperationType.Divide)
                    { Line = line, Column = col };
            }
            else if (token.Type == TokenType.Modulo)
            {
                var (line, col) = Match(Token.Modulo);

                node = new BinaryExpression(node, PatternExpression(), BinaryOperationType.Modulo)
                    { Line = line, Column = col };
            }

            token = CurrentToken;
        }

        return node;
    }
}
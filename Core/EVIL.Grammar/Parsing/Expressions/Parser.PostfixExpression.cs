namespace EVIL.Grammar.Parsing;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private readonly List<TokenType> _postfixOperators =
    [
        TokenType.LParenthesis,
        TokenType.LBracket,
        TokenType.Dot,
        TokenType.Elvis,
        TokenType.ElvisArray,
        TokenType.DoubleColon,
        TokenType.Increment,
        TokenType.Decrement
    ];

    private Expression PostfixExpression()
    {
        var node = PrimaryExpression();
        var token = CurrentToken;
            
        __incdec:
        switch (token.Type)
        {
            case TokenType.Increment:
            {
                var (line, col) = Match(Token.Increment);
                
                return new IncrementationExpression(node, false)
                    { Line = line, Column = col };
            }
            case TokenType.Decrement:
            {
                var (line, col) = Match(Token.Decrement);
                
                return new DecrementationExpression(node, false) 
                    { Line = line, Column = col};
            }
        }

        while (_postfixOperators.Contains(token.Type))
        {
            switch (token.Type)
            {
                case TokenType.Increment or TokenType.Decrement:
                {
                    goto __incdec;
                }

                case TokenType.LParenthesis:
                {
                    node = InvocationExpression(node);
                    break;
                }

                case TokenType.LBracket:
                case TokenType.Dot:
                case TokenType.Elvis:
                case TokenType.ElvisArray:
                {
                    node = IndexerExpression(node);
                    break;
                }

                case TokenType.DoubleColon:
                {
                    node = SelfInvocationExpression(node);
                    break;
                }
            }

            token = CurrentToken;
        }

        return node;
    }
}
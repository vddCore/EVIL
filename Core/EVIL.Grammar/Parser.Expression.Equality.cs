namespace EVIL.Grammar;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private readonly List<TokenType> _equalityOperators =
    [
        TokenType.Equal,
        TokenType.NotEqual,
        TokenType.DeepEqual,
        TokenType.DeepNotEqual
    ];

    private Expression EqualityExpression()
    {
        var node = RelationalExpression();
        var token = CurrentToken;

        while (_equalityOperators.Contains(token.Type))
        {
            switch (token.Type)
            {
                case TokenType.Equal:
                {
                    var (line, col) = Match(Token.Equal);

                    node = new BinaryExpression(node, RelationalExpression(), BinaryOperationType.Equal)
                        { Line = line, Column = col };
                    break;
                }
                
                case TokenType.NotEqual:
                {
                    var (line, col) = Match(Token.NotEqual);

                    node = new BinaryExpression(node, RelationalExpression(), BinaryOperationType.NotEqual)
                        { Line = line, Column = col };
                    break;
                }
                
                case TokenType.DeepEqual:
                {
                    var (line, col) = Match(Token.DeepEqual);

                    node = new BinaryExpression(node, RelationalExpression(), BinaryOperationType.DeepEqual)
                        { Line = line, Column = col };
                    break;
                }
                
                case TokenType.DeepNotEqual:
                {
                    var (line, col) = Match(Token.DeepNotEqual);

                    node = new BinaryExpression(node, RelationalExpression(), BinaryOperationType.DeepNotEqual)
                        { Line = line, Column = col };
                    break;
                }
            }

            token = CurrentToken;
        }

        return node;
    }
}
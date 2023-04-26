using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private List<TokenType> _equalityOperators = new()
        {
            TokenType.Equal,
            TokenType.NotEqual,
            TokenType.DeepEqual,
            TokenType.DeepNotEqual
        };

        private Expression EqualityExpression()
        {
            var node = RelationalExpression();
            var token = CurrentToken;

            while (_equalityOperators.Contains(token.Type))
            {
                if (token.Type == TokenType.Equal)
                {
                    var (line, col) = Match(Token.Equal);

                    node = new BinaryExpression(node, RelationalExpression(), BinaryOperationType.Equal)
                        { Line = line, Column = col };
                }
                else if (token.Type == TokenType.NotEqual)
                {
                    var (line, col) = Match(Token.NotEqual);

                    node = new BinaryExpression(node, RelationalExpression(), BinaryOperationType.NotEqual)
                        { Line = line, Column = col };
                }
                else if (token.Type == TokenType.DeepEqual)
                {
                    var (line, col) = Match(Token.DeepEqual);

                    node = new BinaryExpression(node, RelationalExpression(), BinaryOperationType.DeepEqual)
                        { Line = line, Column = col };
                }
                else if (token.Type == TokenType.DeepNotEqual)
                {
                    var (line, col) = Match(Token.DeepNotEqual);

                    node = new BinaryExpression(node, RelationalExpression(), BinaryOperationType.DeepNotEqual)
                        { Line = line, Column = col };
                }

                token = CurrentToken;
            }

            return node;
        }
    }
}
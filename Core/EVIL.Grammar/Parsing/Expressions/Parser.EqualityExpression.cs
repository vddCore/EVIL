using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private List<TokenType> _equalityOperators = new()
        {
            TokenType.Equal,
            TokenType.NotEqual
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

                token = CurrentToken;
            }

            return node;
        }
    }
}
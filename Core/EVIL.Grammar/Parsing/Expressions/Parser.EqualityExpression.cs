using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
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
            
            if (token.Type == TokenType.Is || token.Type == TokenType.IsNot)
            {
                var (line, col) = (-1, -1);

                var invert = false;

                if (token.Type == TokenType.Is)
                {
                    (line, col) = Match(Token.Is);
                }
                else
                {
                    (line, col) = Match(Token.IsNot);
                    invert = true;
                }

                var right = Constant();

                if (right is not TypeCodeConstant typeCodeConstant)
                {
                    throw new ParserException(
                        "Expected a type code constant.",
                        (right.Line, right.Column)
                    );
                }

                return new IsExpression(node, typeCodeConstant, invert)
                    { Line = line, Column = col };
            }

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
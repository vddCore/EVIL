using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private List<TokenType> _shiftOperators = new()
        {
            TokenType.ShiftLeft,
            TokenType.ShiftRight
        };

        private Expression ShiftExpression()
        {
            var node = AdditiveExpression();
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
}
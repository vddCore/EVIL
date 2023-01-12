using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
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
                    var line = Match(Token.ShiftLeft);
                    node = new BinaryExpression(node, AdditiveExpression(), BinaryOperationType.ShiftLeft)
                        {Line = line};
                }
                else if (token.Type == TokenType.ShiftRight)
                {
                    var line = Match(Token.ShiftRight);
                    node = new BinaryExpression(node, AdditiveExpression(), BinaryOperationType.ShiftRight)
                        {Line = line};
                }
                
                token = CurrentToken;
            }

            return node;
        }
    }
}
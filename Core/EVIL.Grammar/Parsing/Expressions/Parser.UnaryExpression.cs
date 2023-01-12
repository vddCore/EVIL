using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private List<TokenType> _unaryOperators = new()
        {
            TokenType.Plus,
            TokenType.Minus,
            TokenType.LogicalNot,
            TokenType.BitwiseNot,
            TokenType.NameOf,
            TokenType.Length,
            TokenType.Increment,
            TokenType.Decrement,
            TokenType.AsNumber,
            TokenType.AsString
        };

        private Expression UnaryExpression()
        {
            var token = CurrentToken;

            if (token.Type == TokenType.AsString)
            {
                var line = Match(Token.AsString);
                return new UnaryExpression(MultiplicativeExpression(), UnaryOperationType.ToString) { Line = line };
            }
            else if (token.Type == TokenType.Plus)
            {
                var line = Match(Token.Plus);
                return new UnaryExpression(MultiplicativeExpression(), UnaryOperationType.Plus) { Line = line };
            }
            else if (token.Type == TokenType.Minus)
            {
                var line = Match(Token.Minus);
                return new UnaryExpression(MultiplicativeExpression(), UnaryOperationType.Minus) { Line = line };
            }
            else if (token.Type == TokenType.LogicalNot)
            {
                var line = Match(Token.LogicalNot);
                return new UnaryExpression(MultiplicativeExpression(), UnaryOperationType.Negation)
                    { Line = line };
            }
            else if (token.Type == TokenType.BitwiseNot)
            {
                var line = Match(Token.BitwiseNot);
                return new UnaryExpression(MultiplicativeExpression(), UnaryOperationType.BitwiseNot)
                    { Line = line };
            }
            else if (token.Type == TokenType.AsNumber)
            {
                var line = Match(Token.AsNumber);
                return new UnaryExpression(MultiplicativeExpression(), UnaryOperationType.ToNumber) { Line = line };
            }

            return PostfixExpression();
        }
    }
}
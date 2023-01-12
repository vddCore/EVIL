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
            TokenType.Floor,
            TokenType.NameOf,
            TokenType.Length,
            TokenType.Increment,
            TokenType.Decrement,
            TokenType.AsString
        };

        private Expression UnaryExpression()
        {
            var token = CurrentToken;

            if (token.Type == TokenType.NameOf)
            {
                var line = Match(Token.NameOf);
                return new UnaryExpression(PostModificationExpression(), UnaryOperationType.NameOf) {Line = line};
            }
            else if (token.Type == TokenType.Increment)
            {
                DisallowPrevious(TokenType.Decrement, TokenType.Increment);

                var line = Match(Token.Increment);
                return new IncrementationExpression(PostModificationExpression(), true) {Line = line};
            }
            else if (token.Type == TokenType.Decrement)
            {
                DisallowPrevious(TokenType.Decrement, TokenType.Increment);

                var line = Match(Token.Decrement);
                return new DecrementationExpression(PostModificationExpression(), true) {Line = line};
            }
            else if (token.Type == TokenType.Length)
            {
                var line = Match(Token.Length);
                return new UnaryExpression(PostModificationExpression(), UnaryOperationType.Length) {Line = line};
            }
            else if (token.Type == TokenType.AsString)
            {
                var line = Match(Token.AsString);
                return new UnaryExpression(MultiplicativeExpression(), UnaryOperationType.ToString) {Line = line};
            }
            else if (token.Type == TokenType.Plus)
            {
                var line = Match(Token.Plus);
                return new UnaryExpression(MultiplicativeExpression(), UnaryOperationType.Plus) {Line = line};
            }
            else if (token.Type == TokenType.Minus)
            {
                var line = Match(Token.Minus);
                return new UnaryExpression(MultiplicativeExpression(), UnaryOperationType.Minus) {Line = line};
            }
            else if (token.Type == TokenType.LogicalNot)
            {
                var line = Match(Token.LogicalNot);
                return new UnaryExpression(MultiplicativeExpression(), UnaryOperationType.Negation)
                    {Line = line};
            }
            else if (token.Type == TokenType.BitwiseNot)
            {
                var line = Match(Token.BitwiseNot);
                return new UnaryExpression(MultiplicativeExpression(), UnaryOperationType.BitwiseNot)
                    {Line = line};
            }
            else if (token.Type == TokenType.Floor)
            {
                var line = Match(Token.Floor);
                return new UnaryExpression(MultiplicativeExpression(), UnaryOperationType.Floor) {Line = line};
            }

            return PostModificationExpression();
        }
    }
}
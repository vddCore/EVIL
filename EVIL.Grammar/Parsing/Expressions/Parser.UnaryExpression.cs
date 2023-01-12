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
            TokenType.ToString
        };

        private AstNode UnaryExpression()
        {
            var token = CurrentToken;

            if (token.Type == TokenType.NameOf)
            {
                var line = Match(TokenType.NameOf);
                return new UnaryOperationNode(PostModificationExpression(), UnaryOperationType.NameOf) {Line = line};
            }
            else if (token.Type == TokenType.Increment)
            {
                DisallowPrevious(TokenType.Decrement, TokenType.Increment);

                var line = Match(TokenType.Increment);
                return new IncrementationNode(PostModificationExpression(), true) {Line = line};
            }
            else if (token.Type == TokenType.Decrement)
            {
                DisallowPrevious(TokenType.Decrement, TokenType.Increment);

                var line = Match(TokenType.Decrement);
                return new DecrementationNode(PostModificationExpression(), true) {Line = line};
            }
            else if (token.Type == TokenType.Length)
            {
                var line = Match(TokenType.Length);
                return new UnaryOperationNode(PostModificationExpression(), UnaryOperationType.Length) {Line = line};
            }
            else if (token.Type == TokenType.ToString)
            {
                var line = Match(TokenType.ToString);
                return new UnaryOperationNode(MultiplicativeExpression(), UnaryOperationType.ToString) {Line = line};
            }
            else if (token.Type == TokenType.Plus)
            {
                var line = Match(TokenType.Plus);
                return new UnaryOperationNode(MultiplicativeExpression(), UnaryOperationType.Plus) {Line = line};
            }
            else if (token.Type == TokenType.Minus)
            {
                var line = Match(TokenType.Minus);
                return new UnaryOperationNode(MultiplicativeExpression(), UnaryOperationType.Minus) {Line = line};
            }
            else if (token.Type == TokenType.LogicalNot)
            {
                var line = Match(TokenType.LogicalNot);
                return new UnaryOperationNode(MultiplicativeExpression(), UnaryOperationType.Negation)
                    {Line = line};
            }
            else if (token.Type == TokenType.BitwiseNot)
            {
                var line = Match(TokenType.BitwiseNot);
                return new UnaryOperationNode(MultiplicativeExpression(), UnaryOperationType.BitwiseNot)
                    {Line = line};
            }
            else if (token.Type == TokenType.Floor)
            {
                var line = Match(TokenType.Floor);
                return new UnaryOperationNode(MultiplicativeExpression(), UnaryOperationType.Floor) {Line = line};
            }

            return PostModificationExpression();
        }
    }
}
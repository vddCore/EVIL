using EVIL.AST.Base;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private AstNode Terminal()
        {
            var token = Scanner.State.CurrentToken;

            if (token.Type == TokenType.Plus)
            {
                var line = Match(TokenType.Plus);
                return new UnaryOperationNode(Factor(), UnaryOperationType.Plus) {Line = line};
            }
            else if (token.Type == TokenType.Minus)
            {
                var line = Match(TokenType.Minus);
                return new UnaryOperationNode(Factor(), UnaryOperationType.Minus) {Line = line};
            }
            else if (token.Type == TokenType.Increment)
            {
                DisallowPrevious(TokenType.Decrement, TokenType.Increment);

                var line = Match(TokenType.Increment);
                return new IncrementationNode(Factor(), true) {Line = line};
            }
            else if (token.Type == TokenType.Decrement)
            {
                DisallowPrevious(TokenType.Decrement, TokenType.Increment);

                var line = Match(TokenType.Decrement);
                return new DecrementationNode(Factor(), true) {Line = line};
            }
            else if (token.Type == TokenType.NameOf)
            {
                var line = Match(TokenType.NameOf);
                return new UnaryOperationNode(Terminal(), UnaryOperationType.NameOf) {Line = line};
            }
            else if (token.Type == TokenType.Negation)
            {
                var line = Match(TokenType.Negation);
                return new UnaryOperationNode(Factor(), UnaryOperationType.Negation) {Line = line};
            }
            else if (token.Type == TokenType.BitwiseNot)
            {
                var line = Match(TokenType.BitwiseNot);
                return new UnaryOperationNode(Factor(), UnaryOperationType.BitwiseNot) {Line = line};
            }
            else if (token.Type == TokenType.Length)
            {
                var line = Match(TokenType.Length);
                return new UnaryOperationNode(Factor(), UnaryOperationType.Length) {Line = line};
            }
            else if (token.Type == TokenType.ToString)
            {
                var line = Match(TokenType.ToString);
                return new UnaryOperationNode(Factor(), UnaryOperationType.ToString) {Line = line};
            }
            else if (token.Type == TokenType.DecimalNumber)
            {
                var line = Match(TokenType.DecimalNumber);
                return new NumberNode((double)token.Value) { Line = line };
            }
            else if (token.Type == TokenType.HexNumber)
            {
                var line = Match(TokenType.HexNumber);
                return new NumberNode((int)token.Value) { Line = line };
            }
            else if (token.Type == TokenType.LBrace)
            {
                return TableCreation();
            }
            else if (token.Type == TokenType.True)
            {
                var line = Match(TokenType.True);
                return new NumberNode(1) { Line = line };
            }
            else if (token.Type == TokenType.False)
            {
                var line = Match(TokenType.False);
                return new NumberNode(0) { Line = line };
            }
            else if (token.Type == TokenType.String)
            {
                var line = Match(TokenType.String);
                return new StringNode((string)token.Value) { Line = line };
            }
            else if (token.Type == TokenType.LParenthesis)
            {
                var line = Match(TokenType.LParenthesis);

                var node = Assignment();
                node.Line = line;

                Match(TokenType.RParenthesis);

                return node;
            }
            else if (token.Type == TokenType.Fn)
            {
                return FunctionDefinition();
            }
            else if (token.Type == TokenType.Identifier)
            {
                return Variable();
            }
            throw new ParserException($"Unexpected terminal token '{token}'.");
        }
    }
}
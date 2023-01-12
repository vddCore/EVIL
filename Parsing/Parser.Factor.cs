using EVIL.AST.Base;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private AstNode Factor()
        {
            var token = Scanner.State.CurrentToken;

            if (token.Type == TokenType.DecimalNumber)
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
                var line = Match(TokenType.LBrace);
                Match(TokenType.RBrace);
                return new TableNode() { Line = line };
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
            else if (token.Type == TokenType.Plus)
            {
                var line = Match(TokenType.Plus);
                return new UnaryOperationNode(Factor(), UnaryOperationType.Plus) { Line = line };
            }
            else if (token.Type == TokenType.Minus)
            {
                var line = Match(TokenType.Minus);
                return new UnaryOperationNode(Factor(), UnaryOperationType.Minus) { Line = line };
            }
            else if (token.Type == TokenType.Negation)
            {
                var line = Match(TokenType.Negation);
                return new UnaryOperationNode(Factor(), UnaryOperationType.Negation) { Line = line };
            }
            else if (token.Type == TokenType.NameOf)
            {
                var line = Match(TokenType.NameOf);
                return new UnaryOperationNode(Factor(), UnaryOperationType.NameOf) { Line = line };
            }
            else if (token.Type == TokenType.ToString)
            {
                var line = Match(TokenType.ToString);
                return new UnaryOperationNode(Factor(), UnaryOperationType.ToString) { Line = line };
            }
            else if (token.Type == TokenType.Length)
            {
                var line = Match(TokenType.Length);
                return new UnaryOperationNode(Factor(), UnaryOperationType.Length) { Line = line };
            }
            else if (token.Type == TokenType.LParenthesis)
            {
                var line = Match(TokenType.LParenthesis);

                var node = LogicalExpression();
                node.Line = line;

                Match(TokenType.RParenthesis);

                return node;
            }
            else if (token.Type == TokenType.LBracket)
            {
                var state = Scanner.CopyState();
                var node = MemoryGet();
            
                if (Scanner.State.CurrentToken.Type == TokenType.Increment || Scanner.State.CurrentToken.Type == TokenType.Decrement)
                {
                    Scanner.State = state;
                    return MemorySet();
                }
                else
                {
                    return node;
                }
            }
            else if (token.Type == TokenType.Fn)
            {
                return FunctionDefinition();
            }
            else if (token.Type == TokenType.Identifier)
            {
                var identifier = (string)token.Value;
                Match(TokenType.Identifier);

                token = Scanner.State.CurrentToken;

                if (token.Type == TokenType.Assign)
                    return Assignment(identifier);
                else if (token.Type == TokenType.LParenthesis)
                    return FunctionCall(identifier);
                else if (token.Type == TokenType.LBracket)
                    return Indexing(Variable(identifier));
                else if (token.Type == TokenType.Increment)
                    return PostIncrementation(identifier);
                else if (token.Type == TokenType.Decrement)
                    return PostDecrementation(identifier);
                else return Variable(identifier);
            }
            else throw new ParserException($"Unexpected expression term '{token.Value}'.", Scanner.State);
        }
    }
}

using System.Globalization;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode Constant()
        {
            var token = CurrentToken;
            
            if (token.Type == TokenType.Decimal)
            {
                var line = Match(TokenType.Decimal);
                return new DecimalNode(decimal.Parse(token.Value)) {Line = line};
            }
            else if (token.Type == TokenType.HexInteger)
            {
                var line = Match(TokenType.HexInteger);
                return new IntegerNode(int.Parse(token.Value, NumberStyles.HexNumber)) {Line = line};
            }
            else if (token.Type == TokenType.Integer)
            {
                var line = Match(TokenType.Integer);
                return new IntegerNode(int.Parse(token.Value)) {Line = line};
            }
            else if (token.Type == TokenType.True)
            {
                var line = Match(TokenType.True);
                return new IntegerNode(1) {Line = line};
            }
            else if (token.Type == TokenType.False)
            {
                var line = Match(TokenType.False);
                return new IntegerNode(0) {Line = line};
            }
            else if (token.Type == TokenType.String)
            {
                var line = Match(TokenType.String);
                return new StringNode(token.Value.ToString()) {Line = line};
            }
            else throw new ParserException($"Unexpected token [{token}]", Lexer.State);
        }
    }
}
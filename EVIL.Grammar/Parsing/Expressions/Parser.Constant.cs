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

            switch (token.Type)
            {
                case TokenType.Double:
                {
                    var line = Match(TokenType.Double);
                    return new NumberNode(double.Parse(token.Value)) {Line = line};
                }
                case TokenType.HexInteger:
                {
                    var line = Match(TokenType.HexInteger);
                    return new NumberNode(int.Parse(token.Value, NumberStyles.HexNumber)) {Line = line};
                }
                case TokenType.Integer:
                {
                    var line = Match(TokenType.Integer);
                    return new NumberNode(int.Parse(token.Value)) {Line = line};
                }
                case TokenType.True:
                {
                    var line = Match(TokenType.True);
                    return new NumberNode(1) {Line = line};
                }
                case TokenType.False:
                {
                    var line = Match(TokenType.False);
                    return new NumberNode(0) {Line = line};
                }
                case TokenType.String:
                {
                    var line = Match(TokenType.String);
                    return new StringNode(token.Value) {Line = line};
                }
                default:
                    throw new ParserException($"Unexpected token [{token}]", Lexer.State);
            }
        }
    }
}
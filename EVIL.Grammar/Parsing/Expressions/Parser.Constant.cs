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
                case TokenType.Number:
                {
                    var line = Match(Token.Number);
                    return new NumberNode(double.Parse(token.Value)) {Line = line};
                }
                case TokenType.HexInteger:
                {
                    var line = Match(Token.HexInteger);
                    return new NumberNode(int.Parse(token.Value, NumberStyles.HexNumber)) {Line = line};
                }
                case TokenType.True:
                {
                    var line = Match(Token.True);
                    return new NumberNode(1) {Line = line};
                }
                case TokenType.False:
                {
                    var line = Match(Token.False);
                    return new NumberNode(0) {Line = line};
                }
                case TokenType.String:
                {
                    var line = Match(Token.String);
                    return new StringNode(token.Value) {Line = line};
                }
                default:
                    throw new ParserException($"Unexpected token [{token}]", Lexer.State);
            }
        }
    }
}
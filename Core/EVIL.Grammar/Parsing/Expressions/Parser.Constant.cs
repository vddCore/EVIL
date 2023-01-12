using System.Globalization;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression Constant()
        {
            var token = CurrentToken;

            switch (token.Type)
            {
                case TokenType.Number:
                {
                    var line = Match(Token.Number);
                    return new NumberConstant(double.Parse(token.Value)) {Line = line};
                }
                case TokenType.HexInteger:
                {
                    var line = Match(Token.HexInteger);
                    return new NumberConstant(int.Parse(token.Value, NumberStyles.HexNumber)) {Line = line};
                }
                case TokenType.True:
                {
                    var line = Match(Token.True);
                    return new NumberConstant(1) {Line = line};
                }
                case TokenType.False:
                {
                    var line = Match(Token.False);
                    return new NumberConstant(0) {Line = line};
                }
                case TokenType.String:
                {
                    var line = Match(Token.String);
                    return new StringConstant(token.Value) {Line = line};
                }
                default:
                    throw new ParserException($"Unexpected token [{token}]", Lexer.State);
            }
        }
    }
}
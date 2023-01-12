using EVIL.AST.Base;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private AstNode UndefineSymbol()
        {
            var line = Match(TokenType.Undef);

            var node = new UndefNode((string)Scanner.State.CurrentToken.Value) {Line = line};
            Match(TokenType.Identifier);

            return node;
        }
    }
}
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

            var node = new UndefNode { Type = UndefineType.Global, Line = line };

            if (Scanner.State.CurrentToken.Type == TokenType.Fn)
            {
                Match(TokenType.Fn);
                node.Type = UndefineType.Function;
            }
            else if (Scanner.State.CurrentToken.Type == TokenType.LocalVar)
            {
                Match(TokenType.LocalVar);
                node.Type = UndefineType.Local;
            }

            node.Name = (string)Scanner.State.CurrentToken.Value;
            Match(TokenType.Identifier);

            return node;
        }
    }
}

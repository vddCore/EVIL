using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode Meta(AstNode node)
        {
            var line = Match(TokenType.Meta);
            var left = node;
            var ident = Scanner.State.CurrentToken.Value;

            Match(TokenType.Identifier);

            return new MetaNode(left, (string)ident) {Line = line};
        }
    }
}
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode MemberAccess(AstNode indexable)
        {
            var line = Match(TokenType.MemberAccess);
            var identifier = (string)Scanner.State.CurrentToken.Value;
            Match(TokenType.Identifier);

            return new MemberAccessNode(indexable, identifier) {Line = line};
        }
    }
}
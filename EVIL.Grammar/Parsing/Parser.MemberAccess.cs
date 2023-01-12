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
            var identifier = Scanner.State.CurrentToken.Value.ToString();
            Match(TokenType.Identifier);

            return new MemberAccessNode(indexable, identifier) {Line = line};
        }
    }
}
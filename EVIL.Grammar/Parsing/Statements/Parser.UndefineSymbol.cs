using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode UndefineSymbol()
        {
            var line = Match(TokenType.Undef);
            return new UndefNode(PostfixExpression()) {Line = line};
        }
    }
}
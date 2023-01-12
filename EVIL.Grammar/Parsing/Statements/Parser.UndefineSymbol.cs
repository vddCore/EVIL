using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private UndefStatement UndefineSymbol()
        {
            var line = Match(Token.Undef);
            return new UndefStatement(PostfixExpression()) {Line = line};
        }
    }
}
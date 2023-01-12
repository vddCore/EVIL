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

            var variable = Variable();
            var node = new UndefNode(variable) {Line = line};
            
            return node;
        }
    }
}
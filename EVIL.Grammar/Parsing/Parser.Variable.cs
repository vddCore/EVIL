using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private VariableNode Variable()
        {
            var identifier = CurrentToken.Value;
            var line = Match(Token.Identifier);

            return new VariableNode(identifier) {Line = line};
        }
    }
}
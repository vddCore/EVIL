using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private VariableNode Variable(string identifier = null)
        {
            var name = identifier;
            var line = Scanner.State.Line;

            if (identifier == null)
            {
                name = Scanner.State.CurrentToken.Value.ToString();
                line = Match(TokenType.Identifier);
            }

            return new VariableNode(name) { Line = line };
        }
    }
}

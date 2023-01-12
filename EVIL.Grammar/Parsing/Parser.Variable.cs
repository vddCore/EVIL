using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private VariableNode Variable(string identifier = null)
        {
            var name = identifier;
            var line = Lexer.State.Line;

            if (identifier == null)
            {
                name = CurrentToken.Value;
                line = Match(TokenType.Identifier);
            }

            return new VariableNode(name) { Line = line };
        }
    }
}

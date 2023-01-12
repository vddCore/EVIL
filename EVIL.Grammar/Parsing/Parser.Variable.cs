using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private VariableNode Variable(string identifier = null, bool isBeingDefined = false)
        {
            var name = identifier;
            var line = Scanner.State.Line;

            if (identifier == null)
            {
                name = (string)Scanner.State.CurrentToken.Value;
                line = Match(TokenType.Identifier);
            }

            return new VariableNode(name, isBeingDefined) { Line = line };
        }
    }
}

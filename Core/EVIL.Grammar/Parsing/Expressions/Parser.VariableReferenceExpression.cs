using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private VariableReferenceExpression VariableReference()
        {
            var identifier = CurrentToken.Value;
            var line = Match(Token.Identifier);

            return new VariableReferenceExpression(identifier) {Line = line};
        }
    }
}
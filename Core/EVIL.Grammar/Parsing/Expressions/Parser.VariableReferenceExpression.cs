using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private VariableReferenceExpression VariableReference()
        {
            var identifier = CurrentToken.Value;
            var (line, col) = Match(Token.Identifier);

            return new VariableReferenceExpression(identifier!)
                { Line = line, Column = col };
        }
    }
}
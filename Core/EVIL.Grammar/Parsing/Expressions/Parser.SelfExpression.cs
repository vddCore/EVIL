using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private SelfExpression SelfExpression()
        {
            var (line, col) = Match(Token.Self);
            return new SelfExpression { Line = line, Column = col };
        }
    }
}
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ThrowStatement ThrowStatement()
        {
            var (line, col) = Match(Token.Throw);
            var expression = AssignmentExpression();

            return new ThrowStatement(expression)
                { Line = line, Column = col };
        }
    }
}
namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Statements;
using EVIL.Core.Lexical;

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
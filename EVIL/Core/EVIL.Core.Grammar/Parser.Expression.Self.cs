namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Lexical;

public partial class Parser
{
    private SelfExpression SelfExpression()
    {
        var (line, col) = Match(Token.Self);
        return new SelfExpression { Line = line, Column = col };
    }
}
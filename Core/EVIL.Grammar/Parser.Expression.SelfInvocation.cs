namespace EVIL.Grammar;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private SelfInvocationExpression SelfInvocationExpression(Expression indexable)
    {
        var (line, col) = Match(Token.DoubleColon);
        var identifier = Identifier();
        var argumentList = ArgumentList();

        return new SelfInvocationExpression(indexable, identifier, argumentList)
            { Line = line, Column = col };
    }
}
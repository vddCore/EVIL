namespace EVIL.Grammar;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private SelfInvocationExpression SelfInvocationExpression(Expression indexable)
    {
        int line, col;
        var isConditional = false;

        if (CurrentToken.Type == TokenType.SafeColon)
        {
            (line, col) = Match(Token.SafeColon);
            isConditional = true;
        }
        else
        {
            (line, col) = Match(Token.Colon);
        }
        
        var identifier = Identifier();
        var argumentList = ArgumentList();

        return new SelfInvocationExpression(indexable, identifier, argumentList, isConditional)
            { Line = line, Column = col };
    }
}

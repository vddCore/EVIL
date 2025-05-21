namespace EVIL.Grammar;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private YieldExpression YieldExpression()
    {
        var (line, col) = Match(Token.Yield);

        Match(Token.LessThan);

        Expression target;
        if (CurrentToken == Token.LBracket)
        {
            Match(Token.LBracket);
            target = AssignmentExpression();
            Match(Token.RBracket);
        }
        else
        {
            target = SymbolReferenceExpression();
        }
        Match(Token.GreaterThan);
            
        var argumentList = ArgumentList();
            
        return new YieldExpression(target, argumentList)
            { Line = line, Column = col };
    }
}
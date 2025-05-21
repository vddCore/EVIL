namespace EVIL.Grammar;

using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private TypeOfExpression TypeOfExpression()
    {
        var (line, col) = Match(Token.TypeOf);

        var isNative = false;
        
        if (CurrentToken.Type == TokenType.LogicalNot)
        {
            Match(Token.LogicalNot);
            isNative = true;
        }
        
        Match(Token.LParenthesis);
        var target = AssignmentExpression();
        Match(Token.RParenthesis);

        return new TypeOfExpression(target, isNative)
            { Line = line, Column = col };
    }
}
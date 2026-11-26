namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Statements;
using EVIL.Core.Lexical;

public partial class Parser
{
    private DoWhileStatement DoWhileStatement()
    {
        var (line, col) = Match(Token.Do);

        var statements = LoopDescent(Statement);
            
        Match(Token.While);
        Match(Token.LParenthesis);
        var conditionExpression = AssignmentExpression();
        Match(Token.RParenthesis);

        return new DoWhileStatement(conditionExpression, statements) 
            { Line = line, Column = col };
    }
}
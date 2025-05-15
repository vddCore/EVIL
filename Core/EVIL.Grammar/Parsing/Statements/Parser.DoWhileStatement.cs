namespace EVIL.Grammar.Parsing;

using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

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
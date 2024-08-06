namespace EVIL.Grammar.Parsing;

using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

public partial class Parser
{
    private WhileStatement WhileStatement()
    {
        var (line, col) = Match(Token.While);
        Match(Token.LParenthesis);
        var condition = AssignmentExpression();
        Match(Token.RParenthesis);

        var statement = LoopDescent(() => Statement());
            
        return new WhileStatement(condition, statement) 
            { Line = line, Column = col };
    }
}
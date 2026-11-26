namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Statements;
using EVIL.Core.Lexical;

public partial class Parser
{
    private ExpressionBodyStatement ExpressionBodyStatement()
    {
        var (line, col) = Match(Token.RightArrow);
            
        var stmt = new ExpressionBodyStatement(AssignmentExpression())
            { Line = line, Column = col };
            
        return stmt;
    }
}
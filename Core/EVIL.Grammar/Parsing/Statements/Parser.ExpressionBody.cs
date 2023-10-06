using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ExpressionBodyStatement ExpressionBody()
        {
            var (line, col) = Match(Token.RightArrow);
            
            var stmt = new ExpressionBodyStatement(AssignmentExpression())
                { Line = line, Column = col };
            
            return stmt;
        }
    }
}
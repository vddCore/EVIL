using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ErrorExpression ErrorExpression()
        {
            var (line, col) = Match(Token.Error);
            var userData = TableExpression();
            
            return new ErrorExpression(userData)
                { Line = line, Column = col };
        }
    }
}
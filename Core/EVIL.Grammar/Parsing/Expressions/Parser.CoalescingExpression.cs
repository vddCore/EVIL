using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression CoalescingExpression()
        {
            var node = LogicalOrExpression();

            if (CurrentToken == Token.DoubleQuestionMark)
            {
                var (line, col) = Match(Token.DoubleQuestionMark);
                return new CoalescingExpression(node, AssignmentExpression())
                    { Line = line, Column = col };
            }
            
            return node;
        }
    }
}
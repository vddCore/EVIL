namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Lexical;

public partial class Parser
{
    private Expression CoalescingExpression()
    {
        var node = LogicalOrExpression();

        if (CurrentToken == Token.DoubleQuestionMark)
        {
            var (line, col) = Match(Token.DoubleQuestionMark);

            if (CurrentToken == Token.Throw)
            {
                return new CoalescingExpression(node, ThrowStatement())
                { Line = line, Column = col };
            }

            return new CoalescingExpression(node, AssignmentExpression())
                { Line = line, Column = col };
        }
            
        return node;
    }
}
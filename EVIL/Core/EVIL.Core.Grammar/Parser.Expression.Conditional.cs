namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Lexical;

public partial class Parser
{
    private Expression ConditionalExpression()
    {
        var node = CoalescingExpression();

        if (CurrentToken.Type == TokenType.QuestionMark)
        {
            var (line, col) = Match(Token.QuestionMark);
            
            _ternaryDepth++;
            Expression trueExpression;
            try
            {
                trueExpression = AssignmentExpression();
            }
            finally
            {
                _ternaryDepth--;
            }
            
            Match(Token.Colon);
            var falseExpression = ConditionalExpression();

            return new ConditionalExpression(node, trueExpression, falseExpression)
                { Line = line, Column = col };
        }

        return node;
    }
}
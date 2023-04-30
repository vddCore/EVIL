using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression RuntimeExpression()
        {
            var token = CurrentToken;

            if (token.Type == TokenType.TypeOf)
            {
                return TypeOfExpression();
            }
            else if (token.Type == TokenType.Yield)
            {
                return YieldExpression();
            }

            return UnaryExpression();
        }

        private TypeOfExpression TypeOfExpression()
        {
            var (line, col) = Match(Token.TypeOf);

            Match(Token.LParenthesis);
            var target = AssignmentExpression();
            Match(Token.RParenthesis);

            return new TypeOfExpression(target)
                { Line = line, Column = col };
        }

        private YieldExpression YieldExpression()
        {
            var (line, col) = Match(Token.Yield);

            Match(Token.LessThan);

            Expression target;
            if (CurrentToken == Token.LBracket)
            {
                Match(Token.LBracket);
                target = AssignmentExpression();
                Match(Token.RBracket);
            }
            else
            {
                target = VariableReference();
            }
            Match(Token.GreaterThan);
            
            var argumentList = ArgumentList();
            
            return new YieldExpression(target, argumentList)
                { Line = line, Column = col };
        }
    }
}

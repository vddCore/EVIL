using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ArrayExpression ArrayExpression()
        {
            var (line, col) = Match(Token.Array);

            Expression? sizeExpression = null;

            if (CurrentToken == Token.LParenthesis)
            {
                Match(Token.LParenthesis);

                if (CurrentToken != Token.RParenthesis)
                {
                    sizeExpression = AssignmentExpression();
                }

                Match(Token.RParenthesis);
            }
            
            var expressions = new List<Expression>();

            if (CurrentToken == Token.LBrace)
            {
                Match(Token.LBrace);
                while (CurrentToken != Token.RBrace)
                {
                    expressions.Add(AssignmentExpression());

                    if (CurrentToken == Token.RBrace)
                    {
                        break;
                    }

                    Match(Token.Comma);
                }
                Match(Token.RBrace);
            }
            else
            {
                if (sizeExpression == null)
                {
                    throw new ParserException(
                        "Inferred array size requires an array initializer.",
                        (CurrentState.Line, CurrentState.Column)
                    );
                }
            }
            
            return new ArrayExpression(sizeExpression, expressions)
                { Line = line, Column = col };
        }
    }
}
using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private FunctionCallExpression FunctionCall(Expression callee)
        {
            var (line, col) = Match(Token.LParenthesis);

            if (callee is NilConstant)
            {
                throw new ParserException(
                    "'nil' is not a valid invocation target.",
                    (callee.Line, callee.Column)
                );
            }
            
            var arguments = new List<Expression>();

            while (CurrentToken.Type != TokenType.RParenthesis)
            {
                if (CurrentToken.Type == TokenType.EOF)
                {
                    throw new ParserException(
                        $"Unexpected EOF in argument list.",
                        (line, col)
                    );
                }

                arguments.Add(AssignmentExpression());

                if (CurrentToken.Type == TokenType.RParenthesis)
                    break;

                Match(Token.Comma);
            }
            Match(Token.RParenthesis);
            
            return new FunctionCallExpression(callee, arguments) { Line = line };
        }
    }
}

using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ArgumentList ArgumentList()
        {
            var arguments = new List<Expression>();

            var (line, col) = Match(Token.LParenthesis);
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

            return new ArgumentList(arguments)
                { Line = line, Column = col };
        }
    }
}
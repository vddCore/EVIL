using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ReturnStatement Return()
        {
            var (line, col) = Match(Token.Ret);
            Expression expression;

            if (CurrentToken.Type == TokenType.Semicolon)
            {
                expression = null;
            }
            else
            {
                expression = AssignmentExpression();
            }

            return new ReturnStatement(expression)
                { Line = line, Column = col };
        }
    }
}
namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Statements;
using EVIL.Core.Lexical;

public partial class Parser
{
    private RetStatement ReturnStatement()
    {
        var (line, col) = Match(Token.Ret);
        Expression? expression;

        if (CurrentToken.Type == TokenType.Semicolon)
        {
            expression = null;
        }
        else
        {
            expression = AssignmentExpression();
        }

        return new RetStatement(expression)
            { Line = line, Column = col };
    }
}
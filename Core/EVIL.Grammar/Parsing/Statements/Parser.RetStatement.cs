namespace EVIL.Grammar.Parsing;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

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
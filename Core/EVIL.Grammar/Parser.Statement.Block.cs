namespace EVIL.Grammar;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

public partial class Parser
{
    private BlockStatement BlockStatement()
    {
        var (line, col) = Match(Token.LBrace);

        var statements = new List<Statement>();
        while (CurrentToken.Type != TokenType.RBrace)
        {
            if (CurrentToken.Type == TokenType.EOF)
            {
                throw new ParserException(
                    $"Unexpected EOF in a block statement. Unmatched '{{' on line {line}, column {col}.",
                    (_lexer.State.Column, _lexer.State.Line)
                );
            }

            statements.Add(Statement());
        }
        Match(Token.RBrace);

        return new BlockStatement(statements)
            { Line = line, Column = col };
    }
}
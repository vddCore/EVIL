namespace EVIL.Grammar;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Lexical;

public partial class Parser
{
    private ProgramNode Program()
    {
        var statementList = new List<Statement>();

        if (CurrentToken == Token.Empty)
        {
            throw new ParserException(
                "Internal error: lexer is in an invalid state (current token is empty?).",
                (-1, -1)
            );
        }

        while (CurrentToken.Type != TokenType.EOF)
        {
            statementList.Add(
                Statement()
            );
        }

        return new ProgramNode(statementList);
    }
}
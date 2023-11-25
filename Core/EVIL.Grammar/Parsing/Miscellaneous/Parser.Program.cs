using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ProgramNode Program()
        {
            var statementList = new List<TopLevelStatement>();

            if (CurrentToken == Token.Empty)
            {
                throw new ParserException(
                    "Internal error: lexer is in an invalid state (current token is empty?).",
                    (-1, -1)
                );
            }

            while (CurrentToken.Type != TokenType.EOF)
            {
                statementList.Add(TopLevelStatement());
            }

            return new ProgramNode(statementList);
        }
    }
}
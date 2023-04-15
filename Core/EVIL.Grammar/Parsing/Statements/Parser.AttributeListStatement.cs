using System.Collections.Generic;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AttributeListStatement AttributeList()
        {
            if (_functionDescent > 0)
            {
                throw new ParserException(
                    "Attributes are only valid for top-level functions.",
                    (Lexer.State.Line, Lexer.State.Column)
                );
            }

            var attributes = new List<AttributeStatement>();
            var (line, col) = Match(Token.AttributeList);

            while (true)
            {
                attributes.Add(Attribute());

                if (CurrentToken != Token.Semicolon)
                    break;

                Match(Token.Semicolon);
            }
            
            Match(Token.RBracket);

            return new AttributeListStatement(attributes)
                { Line = line, Column = col };
        }
    }
}
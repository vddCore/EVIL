using System.Collections.Generic;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;
using EVIL.Grammar.AST.Statements.TopLevel;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AttributeListNode AttributeList()
        {
            if (_functionDescent > 0)
            {
                throw new ParserException(
                    "Attributes are only valid for top-level functions.",
                    (_lexer.State.Line, _lexer.State.Column)
                );
            }

            var attributes = new List<AttributeNode>();
            var (line, col) = Match(Token.AttributeList);

            while (true)
            {
                attributes.Add(Attribute());

                if (CurrentToken != Token.Semicolon)
                    break;

                Match(Token.Semicolon);
            }
            
            Match(Token.RBracket);

            return new AttributeListNode(attributes)
                { Line = line, Column = col };
        }
    }
}
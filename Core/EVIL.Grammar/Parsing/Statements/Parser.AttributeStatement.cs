using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AttributeStatement Attribute()
        {
            var name = CurrentToken.Value!;
            var (line, col) = Match(Token.Identifier);

            var attributeValues = new List<AstNode>();

            if (CurrentToken == Token.LParenthesis)
            {
                Match(Token.LParenthesis);
                while (CurrentToken != Token.RParenthesis)
                {
                    attributeValues.Add(Constant());

                    if (CurrentToken != Token.RParenthesis)
                    {
                        Match(Token.Comma);
                    }
                }

                Match(Token.RParenthesis);
            }
            
            return new AttributeStatement(name, attributeValues)
                { Line = line, Column = col };
        }
    }
}
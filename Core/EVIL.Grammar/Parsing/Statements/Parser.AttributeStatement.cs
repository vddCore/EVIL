using System.Collections.Generic;
using System.Linq;
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
            var properties = new Dictionary<string, AstNode>();

            if (CurrentToken == Token.LParenthesis)
            {
                Match(Token.LParenthesis);
                while (CurrentToken != Token.RParenthesis)
                {
                    if (CurrentToken == Token.Identifier)
                    {
                        var identifier = CurrentToken.Value!;
                        Match(Token.Identifier);
                        Match(Token.Assign);
                        properties.Add(identifier, Constant());
                    }
                    else
                    {
                        if (properties.Any())
                        {
                            throw new ParserException(
                                $"Attribute values must appear before any properties.",
                                (Lexer.State.Line, Lexer.State.Column)
                            );
                        }

                        attributeValues.Add(Constant());
                    }

                    if (CurrentToken != Token.RParenthesis)
                    {
                        Match(Token.Comma);
                    }
                }

                Match(Token.RParenthesis);
            }

            return new AttributeStatement(name, attributeValues, properties)
                { Line = line, Column = col };
        }
    }
}
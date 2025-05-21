namespace EVIL.Grammar;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Lexical;

public partial class Parser
{
    private AttributeNode Attribute()
    {
        var attributeIdentifier = Identifier();
        var attributeValues = new List<AstNode>();
        var properties = new Dictionary<IdentifierNode, AstNode>();

        if (CurrentToken == Token.LParenthesis)
        {
            Match(Token.LParenthesis);
            while (CurrentToken != Token.RParenthesis)
            {
                if (CurrentToken == Token.Identifier)
                {
                    var attributePropertyIdentifier = Identifier();
                        
                    Match(Token.Assign);
                    properties.Add(attributePropertyIdentifier, ConstantExpression());
                }
                else
                {
                    if (properties.Count != 0)
                    {
                        throw new ParserException(
                            "Attribute values must appear before any properties.",
                            (_lexer.State.Line, _lexer.State.Column)
                        );
                    }

                    attributeValues.Add(ConstantExpression());
                }

                if (CurrentToken != Token.RParenthesis)
                {
                    Match(Token.Comma);
                }
            }

            Match(Token.RParenthesis);
        }

        return new AttributeNode(attributeIdentifier, attributeValues, properties)
            { Line = attributeIdentifier.Line, Column = attributeIdentifier.Column };
    }
}
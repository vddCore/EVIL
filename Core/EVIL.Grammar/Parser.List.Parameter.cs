namespace EVIL.Grammar;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Lexical;

public partial class Parser
{
    private ParameterList ParameterList()
    {
        var (line, col) = Match(Token.LParenthesis);
        var parameters = new List<ParameterNode>();
        var hasInitializers = false;
        var preceedingComma = false;
        var hasSelf = false;
        
        if (CurrentToken.Type == TokenType.Self)
        {
            var (sline, scol) = Match(Token.Self);        
                    
            if (parameters.Count != 0)
            {
                throw new ParserException(
                    "`self' parameter must reside at the beginning of the parameter list.",
                    (sline, scol)
                );
            }

            hasSelf = true;
            
            if (CurrentToken.Type == TokenType.Comma)
                Match(Token.Comma);
        }
            
        while (CurrentToken.Type != TokenType.RParenthesis)
        {
            var rw = false;
            if (CurrentToken.Type == TokenType.Rw)
            {
                rw = true;
                Match(Token.Rw);
            }
                    
            ConstantExpression? initializer = null;

            var parameterIdentifier = Identifier(
                preceedingComma
                    ? "Expected $expected, got $actual."
                    : "Expected $expected or ')', got $actual."
            );

            if (CurrentToken == Token.Assign)
            {
                Match(Token.Assign);
                initializer = ConstantExpression();
                hasInitializers = true;
            }
            else
            {
                if (hasInitializers)
                {
                    throw new ParserException(
                        "Uninitialized parameters must precede default parameters.",
                        (parameterIdentifier.Line, parameterIdentifier.Column)
                    );
                }
            }

            parameters.Add(
                new ParameterNode(parameterIdentifier, rw, initializer)
                    { Line = parameterIdentifier.Line, Column = parameterIdentifier.Column }
            );

            if (CurrentToken == Token.RParenthesis)
                break;

            Match(Token.Comma, "Expected $expected or ')', got $actual");
            preceedingComma = true;
        }
        Match(Token.RParenthesis);

        return new ParameterList(parameters, hasSelf)
            { Line = line, Column = col };
    }
}
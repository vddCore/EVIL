using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ParameterList ParameterList()
        {
            var (line, col) = Match(Token.LParenthesis);
            var parameters = new List<ParameterNode>();
            var hasInitializers = false;
            var preceedingComma = false;
            
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
                    initializer = Constant();
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

            return new ParameterList(parameters)
                { Line = line, Column = col };
        }
    }
}
using System.Collections.Generic;
using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private AstNode FunctionCall(string identifier)
        {
            var functionName = identifier;

            var line = Match(TokenType.LParenthesis);

            var parameters = new List<AstNode>();

            while (Scanner.State.CurrentToken.Type != TokenType.RParenthesis)
            {
                if (Scanner.State.CurrentToken.Type == TokenType.EOF)
                    throw new ParserException($"Unexpected EOF in the function call stated in line {line}.");

                parameters.Add(Comparison());

                if (Scanner.State.CurrentToken.Type == TokenType.RParenthesis)
                    break;

                Match(TokenType.Comma);
            }

            Match(TokenType.RParenthesis);

            return new FunctionCallNode(functionName, parameters) { Line = line };
        }
    }
}

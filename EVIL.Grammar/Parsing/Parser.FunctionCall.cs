using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode FunctionCall(AstNode left)
        {
            var parameters = FunctionParameterList(out var line);
            return new FunctionCallNode(left, parameters) { Line = line };
        }

        private List<AstNode> FunctionParameterList(out int line)
        {
            line = Match(TokenType.LParenthesis);
            var parameters = new List<AstNode>();

            while (CurrentToken.Type != TokenType.RParenthesis)
            {
                if (CurrentToken.Type == TokenType.EOF)
                    throw new ParserException($"Unexpected EOF in the function call stated in line {line}.");

                parameters.Add(AssignmentExpression());

                if (CurrentToken.Type == TokenType.RParenthesis)
                    break;

                Match(TokenType.Comma);
            }
            Match(TokenType.RParenthesis);
            return parameters;
        }
    }
}

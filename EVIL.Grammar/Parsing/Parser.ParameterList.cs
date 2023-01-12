using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ParameterListNode ParameterList()
        {
            var line = Match(TokenType.LParenthesis);
            var parameterList = new List<string>();

            while (CurrentToken.Type != TokenType.RParenthesis)
            {
                parameterList.Add(CurrentToken.Value);
                Match(TokenType.Identifier);

                if (CurrentToken.Type == TokenType.RParenthesis)
                    break;

                Match(TokenType.Comma);
            }
            Match(TokenType.RParenthesis);

            return new ParameterListNode(parameterList) { Line = line };
        }
    }
}
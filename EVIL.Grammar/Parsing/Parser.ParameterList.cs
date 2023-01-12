using System.Collections.Generic;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ParameterListNode ParameterList()
        {
            var line = Match(Token.LParenthesis);
            var parameterList = new List<string>();

            while (CurrentToken.Type != TokenType.RParenthesis)
            {
                parameterList.Add(CurrentToken.Value);
                Match(Token.Identifier);

                if (CurrentToken.Type == TokenType.RParenthesis)
                    break;

                Match(Token.Comma);
            }
            Match(Token.RParenthesis);

            return new ParameterListNode(parameterList) { Line = line };
        }
    }
}
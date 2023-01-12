using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode FunctionDefinition()
        {         
            var line = Match(TokenType.Fn);
            string procName = null;
            
            if (Scanner.State.CurrentToken.Type == TokenType.Identifier)
            {
                procName = Scanner.State.CurrentToken.Value.ToString();                
                Match(TokenType.Identifier);
            }

            Match(TokenType.LParenthesis);
            var parameterList = new List<string>();

            while (Scanner.State.CurrentToken.Type != TokenType.RParenthesis)
            {
                parameterList.Add(Scanner.State.CurrentToken.Value.ToString());
                Match(TokenType.Identifier);

                if (Scanner.State.CurrentToken.Type == TokenType.RParenthesis)
                    break;

                Match(TokenType.Comma);
            }
            Match(TokenType.RParenthesis);
            
            Match(TokenType.LBrace);
            var statementList = FunctionStatementList();
            Match(TokenType.RBrace);

            return new FunctionDefinitionNode(procName, statementList, parameterList) { Line = line };
        }
    }
}

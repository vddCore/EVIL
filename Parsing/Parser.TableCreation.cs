using System.Collections.Generic;
using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private AstNode TableCreation()
        {
            var line = Match(TokenType.LBrace);
            var initializers = new List<AstNode>();

            while (Scanner.State.CurrentToken.Type != TokenType.RBrace)
            {
                initializers.Add(LogicalExpression());

                if (Scanner.State.CurrentToken.Type == TokenType.RBrace)
                    break;

                Match(TokenType.Comma);
            }
            Match(TokenType.RBrace);
            return new TableNode(initializers) { Line = line };
        }
    }
}
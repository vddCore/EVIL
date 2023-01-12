using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode TableCreation()
        {
            var line = Match(TokenType.LBrace);
            var initializers = new List<AstNode>();

            while (Scanner.State.CurrentToken.Type != TokenType.RBrace)
            {
                initializers.Add(Assignment());

                if (Scanner.State.CurrentToken.Type == TokenType.RBrace)
                    break;

                Match(TokenType.Comma);
            }
            Match(TokenType.RBrace);
            return new TableNode(initializers) { Line = line };
        }
    }
}
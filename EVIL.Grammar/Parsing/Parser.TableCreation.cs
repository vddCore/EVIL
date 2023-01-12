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
            var keyed = false;

            while (Scanner.State.CurrentToken.Type != TokenType.RBrace)
            {
                var value = Assignment();

                if (value is AssignmentNode)
                {
                    keyed = true;
                }

                initializers.Add(value);
                
                if (Scanner.State.CurrentToken.Type == TokenType.RBrace)
                    break;

                Match(TokenType.Comma);
            }


            Match(TokenType.RBrace);
            return new TableNode(initializers, keyed) {Line = line};
        }
    }
}
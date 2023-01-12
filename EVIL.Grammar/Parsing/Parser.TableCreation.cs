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
                AstNode key = null;
                if (Scanner.State.CurrentToken.Type == TokenType.LBracket)
                {
                    keyed = true;
                    key = ComputedKeyExpression();
                    Match(TokenType.Assign);
                }
                else
                {
                    var ahead = Scanner.PeekToken(1);
                    if (ahead.Type == TokenType.Assign)
                    {
                        keyed = true;
                        key = Constant();
                        
                        Match(TokenType.Assign);
                    }
                    else
                    {
                        initializers.Add(PostfixExpression());
                    }
                }

                if (keyed)
                {
                    var value = AssignmentExpression();
                    initializers.Add(new KeyValuePairNode(key, value));
                }
                
                if (Scanner.State.CurrentToken.Type == TokenType.RBrace)
                    break;

                Match(TokenType.Comma);
            }

            Match(TokenType.RBrace);
            return new TableNode(initializers, keyed) {Line = line};
        }

        private AstNode ComputedKeyExpression()
        {
            Match(TokenType.LBracket);
            var computedKey = AssignmentExpression();
            Match(TokenType.RBracket);

            return computedKey;
        }
    }
}
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
            var line = Match(Token.LBrace);
            var initializers = new List<AstNode>();
            var keyed = false;

            while (CurrentToken.Type != TokenType.RBrace)
            {
                AstNode key = null;
                if (CurrentToken.Type == TokenType.LBracket)
                {
                    keyed = true;
                    key = ComputedKeyExpression();
                    Match(Token.Assign);
                }
                else
                {
                    var ahead = Lexer.PeekToken(1);
                    if (ahead.Type == TokenType.Assign)
                    {
                        keyed = true;
                        key = Constant();
                        
                        Match(Token.Assign);
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
                
                if (CurrentToken.Type == TokenType.RBrace)
                    break;

                Match(Token.Comma);
            }

            Match(Token.RBrace);
            return new TableNode(initializers, keyed) {Line = line};
        }

        private AstNode ComputedKeyExpression()
        {
            Match(Token.LBracket);
            var computedKey = AssignmentExpression();
            Match(Token.RBracket);

            return computedKey;
        }
    }
}
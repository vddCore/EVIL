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
                if (initializers.Count == 0)
                {
                    if (CurrentToken.Type == TokenType.LBracket)
                    {
                        keyed = true;
                    }
                    else
                    {
                        var ahead = Lexer.PeekToken(1);
                        
                        if (ahead.Type == TokenType.Associate)
                        {
                            keyed = true;
                        }
                    }
                }

                if (keyed)
                {
                    AstNode key, value;
                    
                    if (CurrentToken.Type == TokenType.LBracket)
                    {
                        key = ComputedKeyExpression();
                        Match(Token.Associate);
                        value = AssignmentExpression();

                        initializers.Add(new KeyValuePairNode(key, value));
                    }
                    else
                    {
                        key = Constant();
                        Match(Token.Associate);
                        value = AssignmentExpression();
                        
                        initializers.Add(new KeyValuePairNode(key, value));
                    }
                }
                else
                {
                    initializers.Add(AssignmentExpression());
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
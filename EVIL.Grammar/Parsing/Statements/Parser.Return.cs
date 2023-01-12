using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode Return()
        {
            if (_functionDescent == 0)
            {
                throw new ParserException("Unexpected 'ret' outside of a function.", Lexer.State);
            }

            var line = Match(Token.Ret);
            AstNode retNode;

            if (CurrentToken.Type == TokenType.Semicolon)
            {
                retNode = null;
            }
            else
            {
                retNode = AssignmentExpression();
            }
            
            return new ReturnNode(retNode) { Line = line };
        }
    }
}

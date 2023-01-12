using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        public AstNode EachLoop()
        {
            var line = Match(TokenType.Each);

            Match(TokenType.LParenthesis);
            var keyVar = Variable();
            Match(TokenType.Comma);
            var valueVar = Variable();

            Match(TokenType.Colon);

            var tableNode = Operator();
            
            Match(TokenType.RParenthesis);
            Match(TokenType.LBrace);

            var statements = LoopStatementList();

            Match(TokenType.RBrace);

            return new EachLoopNode(keyVar, valueVar, tableNode, statements) { Line = line };
        }
    }
}

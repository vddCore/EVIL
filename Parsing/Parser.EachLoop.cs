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
            Match(TokenType.RParenthesis);

            Match(TokenType.Colon);

            AstNode tableNode;
            var identifier = Scanner.State.CurrentToken.Value as string;

            Match(TokenType.Identifier);
            if (Scanner.State.CurrentToken.Type == TokenType.LParenthesis)
            {
                tableNode = FunctionCall(identifier);
            }
            else
            {
                tableNode = Variable(identifier);
            }

            Match(TokenType.Do);

            var statements = LoopStatementList();

            Match(TokenType.End);

            return new EachLoopNode(keyVar, valueVar, tableNode, statements) { Line = line };
        }
    }
}

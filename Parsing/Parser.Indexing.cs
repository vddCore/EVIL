using System.Collections.Generic;
using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        public AstNode Indexing(AstNode indexable)
        {
            var line = -1;
            var keyExpressions = new Queue<AstNode>();

            while (Scanner.State.CurrentToken.Type == TokenType.LBracket)
            {
                if (line < 0)
                    line = Match(TokenType.LBracket);
                else Match(TokenType.LBracket);

                keyExpressions.Enqueue(Assignment());
                Match(TokenType.RBracket);
            }

            return new IndexingNode(indexable, keyExpressions, Scanner.State.CurrentToken.Type == TokenType.Assign) {Line = line};
        }
    }
}
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
            var line = Match(TokenType.LBracket);
            var expr = Assignment();
            Match(TokenType.RBracket);

            return new IndexingNode(indexable, expr, Scanner.State.CurrentToken.Type == TokenType.Assign) {Line = line};
        }
    }
}
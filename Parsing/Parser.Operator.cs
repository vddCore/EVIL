using System.Collections.Generic;
using EVIL.AST.Base;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private static readonly List<TokenType> _actionOperators = new()
        {
            TokenType.Assign
        };

        private AstNode Assignment()
        {
            var node = LogicalExpression();
            var token = Scanner.State.CurrentToken;

            while (_actionOperators.Contains(token.Type))
            {
                token = Scanner.State.CurrentToken;

                if (token.Type == TokenType.Assign)
                    node = Assignment(node);
            }

            return node;
        }
    }
}
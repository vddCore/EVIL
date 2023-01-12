using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode IfCondition()
        {
            var line = Match(TokenType.If);
            Match(TokenType.LParenthesis);

            var expression = AssignmentExpression();

            Match(TokenType.RParenthesis);
            var node = new ConditionNode { Line = line };

            node.IfElifBranches.Add(expression, BlockStatement());

            while (CurrentToken.Type == TokenType.Elif || CurrentToken.Type == TokenType.Else)
            {
                if (CurrentToken.Type == TokenType.Elif)
                {
                    Match(TokenType.Elif);
                    Match(TokenType.LParenthesis);

                    expression = AssignmentExpression();

                    Match(TokenType.RParenthesis);
                    node.IfElifBranches.Add(expression, BlockStatement());
                }
                else if (CurrentToken.Type == TokenType.Else)
                {
                    Match(TokenType.Else);
                    node.ElseBranch = BlockStatement();
                }
                else throw new ParserException($"Expected 'end' or 'else' or 'elif', got '{CurrentToken.Value}'", Lexer.State);
            }
            
            return node;
        }
    }
}

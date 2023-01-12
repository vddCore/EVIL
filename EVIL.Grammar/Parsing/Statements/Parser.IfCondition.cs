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
            Match(TokenType.LBrace);

            var node = new ConditionNode { Line = line };
            node.IfElifBranches.Add(expression, ConditionStatementList());
            Match(TokenType.RBrace);

            while (Scanner.State.CurrentToken.Type == TokenType.Elif || Scanner.State.CurrentToken.Type == TokenType.Else)
            {
                if (Scanner.State.CurrentToken.Type == TokenType.Elif)
                {
                    Match(TokenType.Elif);
                    Match(TokenType.LParenthesis);

                    expression = AssignmentExpression();

                    Match(TokenType.RParenthesis);
                    
                    Match(TokenType.LBrace);
                    node.IfElifBranches.Add(expression, ConditionStatementList());
                    Match(TokenType.RBrace);
                }
                else if (Scanner.State.CurrentToken.Type == TokenType.Else)
                {
                    Match(TokenType.Else);
                    Match(TokenType.LBrace);
                    node.ElseBranch = ConditionStatementList();
                    Match(TokenType.RBrace);

                    return node;
                }
                else throw new ParserException($"Expected 'end' or 'else' or 'elif', got '{Scanner.State.CurrentToken.Value}'", Scanner.State);
            }
            return node;
        }
    }
}

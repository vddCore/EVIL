using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode IfCondition()
        {
            var line = Match(Token.If);
            Match(Token.LParenthesis);

            var expression = AssignmentExpression();

            Match(Token.RParenthesis);
            var node = new ConditionNode { Line = line };

            node.IfElifBranches.Add(expression, Statement());

            while (CurrentToken.Type == TokenType.Elif || CurrentToken.Type == TokenType.Else)
            {
                if (CurrentToken.Type == TokenType.Elif)
                {
                    Match(Token.Elif);
                    Match(Token.LParenthesis);

                    expression = AssignmentExpression();

                    Match(Token.RParenthesis);
                    node.IfElifBranches.Add(expression, Statement());
                }
                else if (CurrentToken.Type == TokenType.Else)
                {
                    Match(Token.Else);
                    node.ElseBranch = Statement();
                }
                else throw new ParserException($"Expected '}}' or 'else' or 'elif', got '{CurrentToken.Value}'", Lexer.State);
            }
            
            return node;
        }
    }
}

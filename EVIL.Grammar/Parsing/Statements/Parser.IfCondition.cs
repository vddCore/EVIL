using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private IfStatement IfCondition()
        {
            var line = Match(Token.If);
            Match(Token.LParenthesis);

            var expression = AssignmentExpression();

            Match(Token.RParenthesis);
            var node = new IfStatement { Line = line };

            node.Conditions.Add(expression);
            node.Statements.Add(Statement());

            while (CurrentToken.Type == TokenType.Elif || CurrentToken.Type == TokenType.Else)
            {
                if (CurrentToken.Type == TokenType.Elif)
                {
                    Match(Token.Elif);
                    Match(Token.LParenthesis);

                    expression = AssignmentExpression();

                    Match(Token.RParenthesis);
                    node.Conditions.Add(expression);
                    node.Statements.Add(Statement());
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

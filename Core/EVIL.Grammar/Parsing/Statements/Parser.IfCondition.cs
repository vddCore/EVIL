using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private IfStatement IfCondition()
        {
            var (line, col) = Match(Token.If);
            Match(Token.LParenthesis);

            var expression = AssignmentExpression();

            Match(Token.RParenthesis);

            var node = new IfStatement
                { Line = line, Column = col };

            node.AddCondition(expression);
            node.AddStatement(
                CurrentToken == Token.RightArrow
                    ? ExpressionBody()
                    : Statement()
            );

            while (CurrentToken.Type == TokenType.Elif || CurrentToken.Type == TokenType.Else)
            {
                if (CurrentToken.Type == TokenType.Elif)
                {
                    Match(Token.Elif);
                    Match(Token.LParenthesis);

                    expression = AssignmentExpression();

                    Match(Token.RParenthesis);
                    node.AddCondition(expression);
                    node.AddStatement(
                        CurrentToken == Token.RightArrow
                            ? ExpressionBody()
                            : Statement()
                    );
                }
                else if (CurrentToken.Type == TokenType.Else)
                {
                    Match(Token.Else);
                    node.SetElseBranch(
                        CurrentToken == Token.RightArrow
                            ? ExpressionBody()
                            : Statement()
                    );
                }
                else
                    throw new ParserException(
                        $"Expected '}}' or 'else' or 'elif', got '{CurrentToken.Value}'",
                        (Lexer.State.Line, Lexer.State.Column)
                    );
            }

            return node;
        }
    }
}
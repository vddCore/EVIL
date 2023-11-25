using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private IfStatement IfStatement()
        {
            var (line, col) = Match(Token.If);
            Match(Token.LParenthesis);

            var expression = AssignmentExpression();

            Match(Token.RParenthesis);

            var node = new IfStatement
                { Line = line, Column = col };

            node.AddCondition(expression);

            if (CurrentToken == Token.RightArrow)
            {
                node.AddStatement(ExpressionBodyStatement());
                Match(Token.Semicolon);
            }
            else
            {
                node.AddStatement(Statement());
            }

            while (CurrentToken.Type == TokenType.Elif || CurrentToken.Type == TokenType.Else)
            {
                if (CurrentToken.Type == TokenType.Elif)
                {
                    Match(Token.Elif);
                    Match(Token.LParenthesis);

                    expression = AssignmentExpression();

                    Match(Token.RParenthesis);
                    node.AddCondition(expression);
                    if (CurrentToken == Token.RightArrow)
                    {
                        node.AddStatement(ExpressionBodyStatement());
                        Match(Token.Semicolon);
                    }
                    else
                    {
                        node.AddStatement(Statement());
                    }
                }
                else if (CurrentToken.Type == TokenType.Else)
                {
                    Match(Token.Else);
                    if (CurrentToken == Token.RightArrow)
                    {
                        node.SetElseBranch(ExpressionBodyStatement());
                        Match(Token.Semicolon);
                    }
                    else
                    {
                        node.SetElseBranch(Statement());
                    }
                }
                else
                    throw new ParserException(
                        $"Expected '}}' or 'else' or 'elif', got '{CurrentToken.Value}'",
                        (_lexer.State.Line, _lexer.State.Column)
                    );
            }

            return node;
        }
    }
}
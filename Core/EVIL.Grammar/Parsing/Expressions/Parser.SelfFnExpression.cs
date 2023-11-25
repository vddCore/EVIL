using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private SelfFnExpression SelfFnExpression()
        {
            var (line, col) = Match(Token.Self);
            Match(Token.DoubleColon);
            Match(Token.Fn);
            var parameterList = ParameterList();

            Statement statement;
            if (CurrentToken == Token.LBrace)
            {
                statement = FunctionDescent(BlockStatement);
            }
            else if (CurrentToken == Token.RightArrow)
            {
                statement = FunctionDescent(ExpressionBodyStatement);
            }
            else
            {
                throw new ParserException(
                    $"Expected '{{' or '->', found '{CurrentToken.Value}',",
                    (_lexer.State.Line, _lexer.State.Column)
                );
            }

            return new SelfFnExpression(parameterList, statement)
                { Line = line, Column = col };
        }
    }
}
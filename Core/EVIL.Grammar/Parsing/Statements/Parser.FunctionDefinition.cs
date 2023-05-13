using System.Text.RegularExpressions;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private FunctionDefinition FunctionDefinition()
        {
            var (line, col) = Match(Token.Fn);
            var funcName = CurrentToken.Value!;

            Match(Token.Identifier);

            var parameterList = ParseParameters();

            Statement statement;
            if (CurrentToken == Token.LBrace)
            {
                statement = FunctionDescent(BlockStatement);
            }
            else if (CurrentToken == Token.RightArrow)
            {
                statement = FunctionDescent(ExpressionBody);
            }
            else
            {
                throw new ParserException(
                    $"Expected '{{' or '->', found '{CurrentToken.Value}',",
                    (Lexer.State.Line, Lexer.State.Column)
                );
            }

            return new FunctionDefinition(funcName, parameterList, statement)
                { Line = line, Column = col };
        }
    }
}
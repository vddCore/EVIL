using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private FunctionDefinition FunctionDefinition()
        {
            var (line, col) = Match(Token.Fn);
            var funcName = CurrentToken.Value;

            Match(Token.Identifier);

            var parameterList = ParseParameters();
            var statements = FunctionDescent(BlockStatement);

            return new FunctionDefinition(funcName!, parameterList, statements) 
                { Line = line, Column = col };
        }
    }
}
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private FunctionDefinition FunctionDefinitionNamed()
        {
            var line = Match(Token.Fn);
            var procName = CurrentToken.Value;

            Match(Token.Identifier);

            var parameterList = ParseParameters();
            var statements = FunctionDescent(BlockStatement);

            return new FunctionDefinition(procName, parameterList, statements) {Line = line};
        }
    }
}
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private FunctionExpression FunctionDefinitionAnonymous()
        {
            var line = Match(Token.Fn);

            var parameters = ParseParameters();
            var statements = FunctionDescent(BlockStatement);

            return new FunctionExpression(parameters, statements) {Line = line};
        }
    }
}
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode FunctionDefinitionAnonymous()
        {
            var line = Match(Token.Fn);

            var parameterList = ParameterList();
            var statements = FunctionDescent(BlockStatement);

            return new FunctionDefinitionAnonymousNode(parameterList, statements) {Line = line};
        }
    }
}
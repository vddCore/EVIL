using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode FunctionDefinitionNamed()
        {
            var line = Match(Token.Fn);
            var procName = CurrentToken.Value;

            Match(Token.Identifier);

            var parameterList = ParameterList();
            var statements = FunctionDescent(BlockStatement);

            return new FunctionDefinitionNamedNode(procName, parameterList, statements) {Line = line};
        }
    }
}
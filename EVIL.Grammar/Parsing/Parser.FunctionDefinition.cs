using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode FunctionDefinition()
        {
            var line = Match(Token.Fn);
            string procName = null;

            if (CurrentToken.Type == TokenType.Identifier)
            {
                procName = CurrentToken.Value;
                Match(Token.Identifier);
            }

            var parameterList = ParameterList();
            var statements = FunctionDescent(BlockStatement);

            return new FunctionDefinitionNode(procName, parameterList, statements) {Line = line};
        }
    }
}
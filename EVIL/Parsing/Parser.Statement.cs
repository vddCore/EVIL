using EVIL.AST.Base;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private AstNode Statement()
        {
            var token = Scanner.State.CurrentToken;

            if (token.Type == TokenType.Var)
                return VariableDefinition();
            else if (token.Type == TokenType.Fn)
                return FunctionDefinition();
            else if (token.Type == TokenType.If)
                return IfCondition();
            else if (token.Type == TokenType.For)
                return ForLoop();
            else if (token.Type == TokenType.While)
                return WhileLoop();
            else if (token.Type == TokenType.Each)
                return EachLoop();
            else if (token.Type == TokenType.Ret)
                return Return();
            else if (token.Type == TokenType.Skip)
                return Skip();
            else if (token.Type == TokenType.Break)
                return Break();
            else if (token.Type == TokenType.Undef)
                return UndefineSymbol();
            else if (token.Type == TokenType.Exit)
                return Exit();
            else return Assignment();
        }
    }
}
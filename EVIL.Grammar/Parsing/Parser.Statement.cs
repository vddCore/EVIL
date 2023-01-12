using EVIL.Grammar.AST;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode Statement()
        {
            var token = Scanner.State.CurrentToken;
            AstNode node;
            if (token.Type == TokenType.Fn)
                return FunctionDefinition();
            else if (token.Type == TokenType.If)
                return IfCondition();
            else if (token.Type == TokenType.For)
                return ForLoop();
            else if (token.Type == TokenType.While)
                return WhileLoop();
            else if (token.Type == TokenType.Each)
                return EachLoop();
            else if (token.Type == TokenType.Var)
            {
                node = VariableDefinition();
                Match(TokenType.Semicolon);
            }
            else if (token.Type == TokenType.Ret)
            {
                node = Return();
                Match(TokenType.Semicolon);
            }
            else if (token.Type == TokenType.Skip)
            {
                node = Skip();
                Match(TokenType.Semicolon);
            }
            else if (token.Type == TokenType.Break)
            {
                node = Break();
                Match(TokenType.Semicolon);
            }
            else if (token.Type == TokenType.Undef)
            {
                node = UndefineSymbol();
                Match(TokenType.Semicolon);
            }
            else if (token.Type == TokenType.Exit)
            {
                node = Exit();
                Match(TokenType.Semicolon);
            }
            else if (token.Type == TokenType.Identifier)
            {
                node = AssignmentExpression();
                Match(TokenType.Semicolon);
            }
            else throw new ParserException($"Expected a statement, found '{token.Value}'.", Scanner.State);

            return node;
        }
    }
}
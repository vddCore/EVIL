using EVIL.Grammar.AST;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode Statement()
        {
            var token = CurrentToken;
            AstNode node;
            
            switch (token.Type)
            {
                case TokenType.Fn:
                    node = FunctionDefinition();
                    break;
                case TokenType.If:
                    node = IfCondition();
                    break;
                case TokenType.For:
                    node = ForLoop();
                    break;
                case TokenType.While:
                    node = WhileLoop();
                    break;
                case TokenType.Each:
                    node = EachLoop();
                    break;
                case TokenType.Do:
                    node = DoWhileLoop();
                    Match(TokenType.Semicolon);
                    break;
                case TokenType.Var:
                    node = VariableDefinition();
                    Match(TokenType.Semicolon);
                    break;
                case TokenType.Ret:
                    node = Return();
                    Match(TokenType.Semicolon);
                    break;
                case TokenType.Skip:
                    node = Skip();
                    Match(TokenType.Semicolon);
                    break;
                case TokenType.Break:
                    node = Break();
                    Match(TokenType.Semicolon);
                    break;
                case TokenType.Undef:
                    node = UndefineSymbol();
                    Match(TokenType.Semicolon);
                    break;
                case TokenType.Exit:
                    node = Exit();
                    Match(TokenType.Semicolon);
                    break;
                case TokenType.Identifier:
                    node = AssignmentExpression();
                    Match(TokenType.Semicolon);
                    break;
                case TokenType.LBrace:
                    node = BlockStatement();
                    break;
                default:
                    throw new ParserException($"Expected a statement, found '{token.Value}'.", Lexer.State);
            }

            return node;
        }
    }
}
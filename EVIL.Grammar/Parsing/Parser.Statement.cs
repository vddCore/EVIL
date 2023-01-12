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
                    return FunctionDefinition();
                
                case TokenType.If:
                    return IfCondition();
                
                case TokenType.For:
                    return ForLoop();
                
                case TokenType.While:
                    return WhileLoop();
                
                case TokenType.Each:
                    return EachLoop();
                
                case TokenType.LBrace:
                    return BlockStatement();
                
                case TokenType.Do:
                    node = DoWhileLoop();
                    break;
                case TokenType.Var:
                    node = VariableDefinition();
                    break;
                case TokenType.Undef:
                    node = UndefineSymbol();
                    break;
                case TokenType.Exit:
                    node = Exit();
                    break;
                case TokenType.Identifier:
                    node = AssignmentExpression();
                    break;
                case TokenType.Ret:
                    node = Return();
                    break;
                case TokenType.Skip:
                    node = Skip();
                    break;
                case TokenType.Break:
                    node = Break();
                    break;
                
                default:
                    throw new ParserException($"Expected a statement, found '{token.Value}'.", Lexer.State);
            }

            Match(Token.Semicolon);
            return node;
        }
    }
}
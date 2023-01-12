using EVIL.Grammar.AST;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Statement Statement()
        {
            var token = CurrentToken;
            Statement node;

            if (!_allowTopLevelStatements && _functionDescent == 0 && token.Type != TokenType.Fn)
            {
                throw new ParserException(
                    "Top-level statements are disallowed.",
                    (Lexer.State.Line, Lexer.State.Column)
                );
            }

            switch (token.Type)
            {
                case TokenType.Fn:
                    return FunctionDefinitionNamed();

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

                case TokenType.Loc:
                    node = LocalDefinition();
                    break;

                case TokenType.Undef:
                    node = UndefineSymbol();
                    break;

                case TokenType.Exit:
                    node = Exit();
                    break;

                case TokenType.Increment:
                case TokenType.Decrement:
                    node = new ExpressionStatement(PrefixExpression());
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
                    node = new ExpressionStatement(AssignmentExpression());
                    break;
            }

            Match(Token.Semicolon);
            return node;
        }
    }
}
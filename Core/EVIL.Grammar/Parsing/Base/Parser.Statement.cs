using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Statement Statement()
        {
            var token = CurrentToken;
            Statement node;

            switch (token.Type)
            {
                case TokenType.AttributeList:
                case TokenType.Fn:
                case TokenType.Loc:
                    return FnStatement();
                
                case TokenType.If:
                    node = IfStatement();
                    break;

                case TokenType.For:
                    node = ForStatement();
                    break;

                case TokenType.While:
                    node = WhileStatement();
                    break;

                case TokenType.Each:
                    node = EachStatement();
                    break;

                case TokenType.LBrace:
                    node = BlockStatement();
                    break;

                case TokenType.Override:
                    node = OverrideStatement();
                    break;

                case TokenType.Do:
                    node = DoWhileStatement();
                    Match(Token.Semicolon);
                    break;

                case TokenType.Rw:
                    node = ReadWriteValStatement();
                    Match(Token.Semicolon);
                    break;

                case TokenType.Val:
                    node = ValStatement(false);
                    Match(Token.Semicolon);
                    break;

                case TokenType.Increment:
                case TokenType.Decrement:
                    node = ExpressionStatement(PrefixExpression());
                    Match(Token.Semicolon);
                    break;

                case TokenType.Ret:
                    node = Return();
                    Match(Token.Semicolon);
                    break;

                case TokenType.Skip:
                    node = Skip();
                    Match(Token.Semicolon);
                    break;

                case TokenType.Break:
                    node = Break();
                    Match(Token.Semicolon);
                    break;

                default:
                {
                    node = new ExpressionStatement(AssignmentExpression());
                    Match(Token.Semicolon);
                    
                    break;
                }
            }

            return node;
        }
    }
}
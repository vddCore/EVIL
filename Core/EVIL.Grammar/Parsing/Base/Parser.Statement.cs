using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private int _semicolonExemptions = 0;
        
        private Statement Statement()
        {
            var token = CurrentToken;
            Statement node;

            switch (token.Type)
            {
                case TokenType.AttributeList:
                case TokenType.Fn:
                    return FnStatement();
                
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
                
                case TokenType.Override:
                    return OverrideStatement();

                case TokenType.Do:
                    node = DoWhileLoop();
                    break;
                
                case TokenType.Rw:
                    node = ReadWriteValStatement();
                    break;

                case TokenType.Val:
                    node = ValStatement(false);
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
                {
                    var expr = AssignmentExpression();

                    if (!expr.IsValidExpressionStatement)
                    {
                        throw new ParserException(
                            "Only assignment, invocation, increment, decrement and yield expressions can be used as statements.",
                            (expr.Line, expr.Column)
                        );
                    }
                    
                    node = new ExpressionStatement(expr);
                    break;    
                }

            }

            if (_semicolonExemptions == 0)
            {
                Match(Token.Semicolon);
            }
            else
            {
                _semicolonExemptions--;
            }

            return node;
        }
    }
}
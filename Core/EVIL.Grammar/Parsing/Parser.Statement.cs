using System.Collections.Generic;
using EVIL.Grammar.AST;
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
                    return AttributeList();

                case TokenType.Fn:
                    return FunctionDefinition();

                case TokenType.If:
                    return IfCondition();

                case TokenType.For:
                    return ForLoop();

                case TokenType.While:
                    return WhileLoop();

                case TokenType.LBrace:
                    return BlockStatement();

                case TokenType.Do:
                    node = DoWhileLoop();
                    break;

                case TokenType.Var:
                    node = VariableDefinition();
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
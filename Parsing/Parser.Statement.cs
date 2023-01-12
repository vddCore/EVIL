using EVIL.AST.Base;
using EVIL.AST.Enums;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private AstNode Statement()
        {
            var token = Scanner.State.CurrentToken;

            if (token.Type == TokenType.Identifier)
            {
                var identifier = (string)token.Value;
                Match(TokenType.Identifier);

                token = Scanner.State.CurrentToken;

                if (token.Type == TokenType.Assign || token.Type == TokenType.LBracket)
                    return Assignment(Variable(identifier));
                else if (token.Type == TokenType.CompoundAdd)
                    return CompoundAssignment(identifier, CompoundAssignmentType.Add);
                else if (token.Type == TokenType.CompoundSubtract)
                    return CompoundAssignment(identifier, CompoundAssignmentType.Subtract);
                else if (token.Type == TokenType.CompoundMultiply)
                    return CompoundAssignment(identifier, CompoundAssignmentType.Multiply);
                else if (token.Type == TokenType.CompoundDivide)
                    return CompoundAssignment(identifier, CompoundAssignmentType.Divide);
                else if (token.Type == TokenType.CompoundModulo)
                    return CompoundAssignment(identifier, CompoundAssignmentType.Modulo);
                else if (token.Type == TokenType.CompoundBitwiseAnd)
                    return CompoundAssignment(identifier, CompoundAssignmentType.BitwiseAnd);
                else if (token.Type == TokenType.CompoundBitwiseOr)
                    return CompoundAssignment(identifier, CompoundAssignmentType.BitwiseOr);
                else if (token.Type == TokenType.CompoundBitwiseXor)
                    return CompoundAssignment(identifier, CompoundAssignmentType.BitwiseXor);
                else if (token.Type == TokenType.LParenthesis)
                    return FunctionCall(Variable(identifier));
                else if (token.Type == TokenType.Increment)
                    return PostIncrementation(Variable(identifier));
                else if (token.Type == TokenType.Decrement)
                    return PostDecrementation(Variable(identifier));
                else throw new ParserException($"Expected an assignment or a function call, found '{token.Value}'.", Scanner.State);
            }
            else if (token.Type == TokenType.Var)
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
            else throw new ParserException($"Expected a statement, found '{token.Value}'.", Scanner.State);
        }
    }
}
using EVIL.AST.Base;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private AstNode CompoundAssignment(string identifier, CompoundAssignmentType type)
        {
            var variable = Variable(identifier);

            int line = 0;
            switch (type)
            {
                case CompoundAssignmentType.Add:
                    Match(TokenType.CompoundAdd);
                    break;

                case CompoundAssignmentType.Subtract:
                    Match(TokenType.CompoundSubtract);
                    break;

                case CompoundAssignmentType.Multiply:
                    Match(TokenType.CompoundMultiply);
                    break;

                case CompoundAssignmentType.Divide:
                    Match(TokenType.CompoundDivide);
                    break;

                case CompoundAssignmentType.Modulo:
                    Match(TokenType.CompoundModulo);
                    break;

                default:
                    throw new ParserException("Unexpected compound assignment type enum value??");
            }

            var right = LogicalExpression();

            return new CompoundAssignmentNode(variable, right, type) { Line = line };
    }
}
}

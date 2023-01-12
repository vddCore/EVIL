using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(TableAssignmentNode tableAssignmentNode)
        {
            var variable = Visit(tableAssignmentNode.Variable);

            if (variable.Type != DynValueType.Table)
                throw new RuntimeException("Attempt to index a non-table value.", tableAssignmentNode.Line);

            var expressionValue = Visit(tableAssignmentNode.ValueExpression);

            if (expressionValue.Type == DynValueType.Table)
                throw new RuntimeException("Cannot nest tables inside tables.", tableAssignmentNode.Line);

            var keyValue = Visit(tableAssignmentNode.KeyExpression);

            variable.Table[keyValue] = expressionValue;

            return expressionValue;
        }
    }
}

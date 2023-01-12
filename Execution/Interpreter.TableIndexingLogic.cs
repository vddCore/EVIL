using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(TableIndexingNode tableIndexingNode)
        {
            var variable = Visit(tableIndexingNode.Variable);

            if (variable.Type != DynValueType.Table)
                throw new RuntimeException("Attempt to index a non-table value.", tableIndexingNode.Line);

            var keyValue = Visit(tableIndexingNode.KeyExpression);

            if (keyValue.Type == DynValueType.String)
                return variable.Table[keyValue.String];
            else if (keyValue.Type == DynValueType.Number)
                return variable.Table[keyValue.Number];
            else throw new RuntimeException($"Type '{keyValue.Type}' cannot be used as a key.", tableIndexingNode.Line);
        }
    }
}

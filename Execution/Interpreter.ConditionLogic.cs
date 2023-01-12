using EVIL.Abstraction;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(ConditionNode conditionNode)
        {
            foreach (var branch in conditionNode.IfElifBranches)
            {
                var exprResult = Visit(branch.Key);

                if (exprResult.Number != 0)
                {
                    ExecuteStatementList(branch.Value);
                    return DynValue.Zero;
                }
            }

            if (conditionNode.ElseBranch != null)
                ExecuteStatementList(conditionNode.ElseBranch);

            return DynValue.Zero;
        }
    }
}

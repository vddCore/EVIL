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

                if (exprResult.IsTruth)
                {
                    Environment.EnterScope();
                    {
                        ExecuteStatementList(branch.Value);
                    }
                    Environment.ExitScope();
                    
                    return DynValue.Zero;
                }
            }

            if (conditionNode.ElseBranch != null)
            {
                Environment.EnterScope();
                {
                    ExecuteStatementList(conditionNode.ElseBranch);
                }
                Environment.ExitScope();
            }

            return DynValue.Zero;
        }
    }
}

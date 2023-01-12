using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
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
                    Visit(branch.Value);
                    return DynValue.Zero;
                }
            }

            if (conditionNode.ElseBranch != null)
            {
                Visit(conditionNode.ElseBranch);
            }

            return DynValue.Zero;
        }
    }
}

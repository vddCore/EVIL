using System.Linq;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(DecisionNode decisionNode)
        {
            for (var i = 0; i < decisionNode.Conditions.Count; i++)
            {
                var exprResult = Visit(decisionNode.Conditions[i]);

                if (exprResult.IsTruth)
                {
                    Visit(decisionNode.Statements[i]);
                    return DynValue.Zero;
                }
            }

            if (decisionNode.ElseBranch != null)
            {
                Visit(decisionNode.ElseBranch);
            }

            return DynValue.Zero;
        }
    }
}
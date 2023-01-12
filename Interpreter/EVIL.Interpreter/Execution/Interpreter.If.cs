using System.Linq;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(IfStatement ifStatement)
        {
            for (var i = 0; i < ifStatement.Conditions.Count; i++)
            {
                var exprResult = Visit(ifStatement.Conditions[i]);

                if (exprResult.IsTruth)
                {
                    Visit(ifStatement.Statements[i]);
                    return;
                }
            }

            if (ifStatement.ElseBranch != null)
            {
                Visit(ifStatement.ElseBranch);
            }
        }
    }
}
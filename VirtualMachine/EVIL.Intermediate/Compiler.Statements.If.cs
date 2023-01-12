using System.Diagnostics;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(IfStatement ifStatement)
        {
            Debug.Assert(ifStatement.Conditions.Count == ifStatement.Statements.Count);

            var cg = CurrentChunk.GetCodeGenerator();

            var endLabel = _executable.DefineLabel();
            for (var i = 0; i < ifStatement.Conditions.Count; i++)
            {
                var cond = ifStatement.Conditions[i];
                var stmt = ifStatement.Statements[i];

                Visit(cond);
                var label = _executable.DefineLabel();
                cg.Emit(OpCode.FJMP, label);
                Visit(stmt);

                if (i < ifStatement.Conditions.Count - 1)
                {
                    cg.Emit(OpCode.JUMP, endLabel);
                }
                else
                {
                    if (ifStatement.ElseBranch != null)
                    {
                        cg.Emit(OpCode.JUMP, endLabel);
                    }
                }

                _executable.UpdateLabel(label, cg.IP);
            }

            if (ifStatement.ElseBranch != null)
            {
                Visit(ifStatement.ElseBranch);
            }

            _executable.UpdateLabel(endLabel, cg.IP);
        }
    }
}
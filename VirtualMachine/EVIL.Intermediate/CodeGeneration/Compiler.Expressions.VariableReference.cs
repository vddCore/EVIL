using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(VariableReferenceExpression variableReferenceExpression)
        {
            var cg = CurrentChunk.GetCodeGenerator();
            EmitVariableLoad(cg, variableReferenceExpression);
        }
    }
}
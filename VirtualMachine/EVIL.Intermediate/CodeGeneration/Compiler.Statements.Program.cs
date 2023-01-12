using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(Program program)
        {
            foreach (var node in program.Statements)
                Visit(node);
        }
    }
}
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(BlockStatement blockStatement)
        {
            EnterScope();
            {
                foreach (var node in blockStatement.Statements)
                    Visit(node);
            }
            LeaveScope();
        }
    }
}
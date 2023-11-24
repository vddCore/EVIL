using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Miscellaneous;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ProgramNode programNode)
        {
            foreach (var node in programNode.Statements)
            {
                Visit(node);
            }
        }
    }
}
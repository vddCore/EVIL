using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Miscellaneous;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ProgramNode programNode)
        {
            foreach (var node in programNode.FnStatements)
            {
                Visit(node);
            }
            
            foreach (var node in programNode.AnythingButFnStatements)
            {
                Visit(node);
            }
        }
    }
}
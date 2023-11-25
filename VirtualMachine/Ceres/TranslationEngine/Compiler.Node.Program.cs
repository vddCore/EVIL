using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Miscellaneous;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ProgramNode programNode)
        {
            for (var i = 0; i < programNode.Statements.Count; i++)
            {
                Visit(programNode.Statements[i]);
            }
        }
    }
}
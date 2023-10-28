using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Miscellaneous;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ProgramNode programNode)
        {
            _script = new Script();
            _closedScopes.Clear();
            
            foreach (var node in programNode.Statements)
            {
                Visit(node);
            }
        }
    }
}
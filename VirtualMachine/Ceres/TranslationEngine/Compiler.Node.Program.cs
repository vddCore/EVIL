using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Scoping;
using EVIL.Grammar.AST.Miscellaneous;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ProgramNode programNode)
        {
            _script = new Script();
            _closedScopes.Clear();
            
            RootScope.Clear();
            _closedScopes.Add(RootScope);

            foreach (var node in programNode.Statements)
            {
                Visit(node);
            }
        }
    }
}
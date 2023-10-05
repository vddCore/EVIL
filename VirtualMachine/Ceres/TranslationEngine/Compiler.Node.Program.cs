using Ceres.ExecutionEngine.Diagnostics;
using EVIL.Grammar.AST.Miscellaneous;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(ProgramNode programNode)
        {
            _script = new Script();
            _rootScope = Scope.CreateRoot();
            _currentScope = _rootScope;

            foreach (var node in programNode.Statements)
            {
                Visit(node);
            }
        }
    }
}
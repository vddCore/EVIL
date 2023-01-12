using EVIL.Abstraction;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(UndefNode undefNode)
        {
            switch (undefNode.Type)
            {
                case UndefineType.Local:
                    if (!Environment.IsInScriptFunctionScope)
                        throw new RuntimeException($"Attempt to undefine a local variable outside a function '{undefNode.Name}'.", undefNode.Line);

                    Environment.LocalScope.UnSet(undefNode.Name);
                    break;
                
                case UndefineType.Global:
                    Environment.GlobalScope.UnSet(undefNode.Name);
                    break;

                default:
                    throw new RuntimeException("Internal interpreter error: unexpected UndefType value.", undefNode.Line);
            }
            return DynValue.Zero;
        }
    }
}

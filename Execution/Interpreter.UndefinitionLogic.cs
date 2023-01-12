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
                    if (CallStack.Count == 0)
                        throw new RuntimeException($"Attempt to undefine a local variable outside a function '{undefNode.Name}'.", undefNode.Line);

                    var callStackItem = CallStack.Peek();

                    if (!callStackItem.LocalVariableScope.ContainsKey(undefNode.Name))
                        throw new RuntimeException($"Attempt to undefine a non-existent local variable '{undefNode.Name}'.", undefNode.Line);

                    callStackItem.LocalVariableScope.Remove(undefNode.Name);
                    break;

                case UndefineType.Function:
                    if (Environment.BuiltIns.ContainsKey(undefNode.Name))
                        throw new RuntimeException($"Attempt to undefine a built-in function '{undefNode.Name}'.", undefNode.Line);

                    if (!Environment.Functions.ContainsKey(undefNode.Name))
                        throw new RuntimeException($"Attempt to undefine a non-existent function '{undefNode.Name}'", undefNode.Line);

                    Environment.Functions.Remove(undefNode.Name);
                    break;

                case UndefineType.Global:
                    if (!Environment.Globals.ContainsKey(undefNode.Name))
                        throw new RuntimeException($"Attempt to undefine a non-existent global variable '{undefNode.Name}'.", undefNode.Line);

                    Environment.Globals.Remove(undefNode.Name);
                    break;

                default:
                    throw new RuntimeException("Internal interpreter error: unexpected UndefType value.", undefNode.Line);
            }
            return DynValue.Zero;
        }
    }
}

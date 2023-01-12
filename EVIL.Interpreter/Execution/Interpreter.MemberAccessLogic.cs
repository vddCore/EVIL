using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(MemberAccessNode memberAccessNode)
        {
            var indexable = Visit(memberAccessNode.Indexable);

            if (indexable.Type != DynValueType.Table)
            {
                throw new RuntimeException(
                    $"Attempt to access named member '{memberAccessNode.Identifier}' of non-indexable type {indexable.Type}",
                    Environment,
                    memberAccessNode.Line
                );
            }

            try
            {
                return indexable.Table[memberAccessNode.Identifier];
            }
            catch
            {
                throw new RuntimeException(
                    $"Member '{memberAccessNode.Identifier}' does not exist in this table.",
                    Environment,
                    memberAccessNode.Line
                );
            }
        }
    }
}
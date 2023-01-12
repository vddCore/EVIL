using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(MetaNode metaNode)
        {
            var left = Visit(metaNode.Left);
            var identifier = metaNode.Identifier;

            return left.Meta[identifier];
        }
    }
}
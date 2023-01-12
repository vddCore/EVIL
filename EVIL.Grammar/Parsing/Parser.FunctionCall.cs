using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode FunctionCall(AstNode left)
        {
            var parameters = ArgumentList(out var line);
            return new FunctionCallNode(left, parameters) { Line = line };
        }
    }
}

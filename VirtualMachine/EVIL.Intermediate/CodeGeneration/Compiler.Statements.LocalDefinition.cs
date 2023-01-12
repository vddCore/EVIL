using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(LocalDefinition localDefinition)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            if (ScopeStack.Count <= 0)
            {
                throw new CompilerException(
                    "`loc` is only valid in a local scope.",
                    CurrentLine,
                    CurrentColumn
                );
            }
            
            var localScope = ScopeStack.Peek();

            foreach (var kvp in localDefinition.Definitions)
            {
                var sym = localScope.DefineLocal(kvp.Key);

                if (kvp.Value != null)
                {
                    Visit(kvp.Value);
                    EmitByteOp(cg, OpCode.STL, (byte)sym.Id);
                }
            }
        }
    }
}
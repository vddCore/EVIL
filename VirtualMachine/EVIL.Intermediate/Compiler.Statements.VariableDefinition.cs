using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(VariableDefinition variableDefinition)
        {
            var cg = CurrentChunk.GetCodeGenerator();

            Scope localScope = null;

            if (ScopeStack.Count > 0)
            {
                localScope = ScopeStack.Peek();
            }

            foreach (var kvp in variableDefinition.Definitions)
            {
                if (localScope != null)
                {
                    var sym = localScope.DefineLocal(kvp.Key);

                    if (kvp.Value != null)
                    {
                        Visit(kvp.Value);
                        cg.Emit(OpCode.STL, sym.Id);
                    }
                }
                else
                {
                    _executable.DefineGlobal(kvp.Key);

                    if (kvp.Value != null)
                    {
                        Visit(kvp.Value);

                        var keyId = _executable.ConstPool.FetchOrAddConstant(kvp.Key);
                        cg.Emit(OpCode.STG, keyId);
                    }
                }
            }
        }
    }
}
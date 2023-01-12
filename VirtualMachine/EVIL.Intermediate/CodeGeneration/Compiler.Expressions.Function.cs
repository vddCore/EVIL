using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
{
    public partial class Compiler
    {
        public override void Visit(FunctionExpression functionExpression)
        {
            var prevCg = CurrentChunk.GetCodeGenerator();
            ScopeStack.TryPeek(out var prevScope);

            var (id, chunk) = CurrentChunk.CreateSubChunk();
            ChunkDefinitionStack.Push(chunk);

            EnterScope();
            {
                var currentScope = ScopeStack.Peek();
                var currentCg = CurrentChunk.GetCodeGenerator();

                while (prevScope != null)
                {
                    foreach (var kvp in prevScope.Symbols.Forward)
                    {
                        var identifier = kvp.Key;
                        var externLocalSym = kvp.Value;

                        if (externLocalSym.Type == SymbolInfo.SymbolType.Local)
                        {
                            currentScope.DefineExtern(identifier, prevScope.Chunk.Name, externLocalSym.Id, false);
                        }
                        else if (externLocalSym.Type == SymbolInfo.SymbolType.Parameter)
                        {
                            currentScope.DefineExtern(identifier, prevScope.Chunk.Name, externLocalSym.Id, true);
                        }
                    }

                    prevScope = prevScope.Parent;
                }

                BuildFunction(
                    currentCg,
                    functionExpression.Parameters,
                    functionExpression.Statements
                );
            }
            LeaveScope();

            ChunkDefinitionStack.Pop();
            prevCg.Emit(OpCode.LDF, id);
        }
    }
}
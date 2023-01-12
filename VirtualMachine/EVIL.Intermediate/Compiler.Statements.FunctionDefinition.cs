using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate
{
    public partial class Compiler
    {
        public override void Visit(FunctionDefinition functionDefinition)
        {
            if (IsLocalScope)
            {
                throw new CompilerException(
                    "Function definition statements are not allowed in a local scope. " +
                    "Perhaps you should use a function expression instead.",
                    functionDefinition.Line,
                    functionDefinition.Column
                );
            }

            _executable.DefineGlobal(functionDefinition.Identifier);

            var chunk = new Chunk(functionDefinition.Identifier);
            ChunkDefinitionStack.Push(chunk);

            var cg = CurrentChunk.GetCodeGenerator();
            
            BuildFunction(
                cg,
                functionDefinition.Parameters,
                functionDefinition.Statements
            );

            _executable.Chunks.Add(ChunkDefinitionStack.Pop());
        }
    }
}
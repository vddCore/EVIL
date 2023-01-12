using System.Linq;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Intermediate.CodeGeneration
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

            var previousDefinition = _executable.Chunks.FirstOrDefault(x => x.Name == functionDefinition.Identifier);
            
            if (previousDefinition != null)
            {
                throw new CompilerException(
                    $"Duplicate '{functionDefinition.Identifier}' function definition, previously defined on line {previousDefinition.DefinedOnLine}.",
                    functionDefinition.Line,
                    functionDefinition.Column
                );
            }
            
            var chunk = new Chunk(
                functionDefinition.Identifier, 
                true, 
                functionDefinition.Line
            );
            
            ChunkDefinitionStack.Push(chunk);
            var cg = CurrentChunk.GetCodeGenerator();
            
            EnterScope();
            {
                BuildFunction(
                    cg,
                    functionDefinition.Parameters,
                    functionDefinition.Statements
                );
            }
            LeaveScope();
            _executable.Chunks.Add(ChunkDefinitionStack.Pop());
        }
    }
}
using Ceres.ExecutionEngine.Diagnostics;
using EVIL.CommonTypes.TypeSystem;
using EVIL.Grammar.AST.Statements;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(OverrideStatement overrideStatement)
        {
            var id = InAnonymousSubChunkDo(() =>
            {
                if (overrideStatement.Override == TableOverride.Invoke
                    || overrideStatement.Override == TableOverride.Get
                    || overrideStatement.Override == TableOverride.Set
                    || overrideStatement.Override == TableOverride.Exists)
                {
                    Chunk.MarkSelfAware();
                    Chunk.AllocateParameter();
                }
                
                InNewClosedScopeDo(() =>
                {
                    Chunk.DebugDatabase.DefinedOnLine = overrideStatement.Line;

                    Visit(overrideStatement.ParameterList);
                    Visit(overrideStatement.Statement);

                    FinalizeChunk();
                });
            });

            Chunk.CodeGenerator.Emit(OpCode.LDCNK, id);
            Visit(overrideStatement.Target);
            Chunk.CodeGenerator.Emit(
                OpCode.OVERRIDE,
                (byte)overrideStatement.Override
            );
        }
    }
}
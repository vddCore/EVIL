namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Statements;
using EVIL.VM.ExecutionEngine.Diagnostics;

public partial class Compiler
{
    protected override void Visit(EachStatement eachStatement)
    {
        InNewLocalScopeDo(() =>
        {
            int keyLocal = Chunk.AllocateLocal();
            int valueLocal = -1;
            var isKeyValue = false;

            try
            {
                CurrentScope.DefineLocal(
                    eachStatement.KeyIdentifier.Name,
                    keyLocal,
                    false,
                    eachStatement.KeyIdentifier.Line,
                    eachStatement.KeyIdentifier.Column
                );
            }
            catch (DuplicateSymbolException dse)
            {
                Log.TerminateWithFatal(
                    CompilerDiagnostics.DuplicateSymbolInScope.Create(
                        GetLocation(eachStatement.KeyIdentifier),
                        [new RelatedLocation(new SourceLocation(CurrentFileName, dse.Line, dse.Column), "Previously defined here")],
                        eachStatement.KeyIdentifier.Name,
                        dse.ExistingSymbol.TypeName,
                        dse.Line,
                        dse.Column
                    ),
                    dse
                );

                // Return just in case - TerminateWithFatal should never return, ever.
                return;
            }

            if (eachStatement.ValueIdentifier != null)
            {
                valueLocal = Chunk.AllocateLocal();

                try
                {
                    CurrentScope.DefineLocal(
                        eachStatement.ValueIdentifier.Name,
                        valueLocal,
                        false,
                        eachStatement.ValueIdentifier.Line,
                        eachStatement.ValueIdentifier.Column
                    );
                }
                catch (DuplicateSymbolException dse)
                {
                    Log.TerminateWithFatal(
                        CompilerDiagnostics.DuplicateSymbolInScope.Create(
                            GetLocation(eachStatement.ValueIdentifier),
                            [new RelatedLocation(new SourceLocation(CurrentFileName, dse.Line, dse.Column), "Previously defined here")],
                            eachStatement.ValueIdentifier.Name,
                            dse.ExistingSymbol.TypeName,
                            dse.Line,
                            dse.Column
                        ),
                        dse
                    );

                    // Return just in case - TerminateWithFatal should never return, ever.
                    return;
                }

                isKeyValue = true;
            }

            Visit(eachStatement.Iterable);
            Chunk.CodeGenerator.Emit(OpCode.EACH);
                
            InNewLoopDo(Loop.LoopKind.Each, () =>
            {
                Chunk.UpdateLabel(Loop.StartLabel, Chunk.CodeGenerator.IP);
                Chunk.CodeGenerator.Emit(OpCode.NEXT, isKeyValue ? 1 : 0);
                Chunk.CodeGenerator.Emit(OpCode.FJMP, Loop.EndLabel);
                Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, keyLocal);

                if (isKeyValue)
                {
                    Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, valueLocal);
                }

                Visit(eachStatement.Statement);
                
                if (!_isUnreachable)
                {
                    Chunk.CodeGenerator.Emit(OpCode.JUMP, Loop.StartLabel);
                }
                
                Chunk.UpdateLabel(Loop.EndLabel, Chunk.CodeGenerator.IP);
                Chunk.CodeGenerator.Emit(OpCode.EEND);
            }, false);
        });
    }
}
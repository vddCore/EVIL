namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Miscellaneous;

public partial class Compiler
{
    protected override void Visit(ParameterList parameterList)
    {
        if (parameterList.HasSelf && !Chunk.IsSelfAware)
        {
            Chunk.AllocateParameter();
            Chunk.MarkSelfAware();
        }

        for (var i = 0; i < parameterList.Parameters.Count; i++)
        {
            var parameter = parameterList.Parameters[i];
            var parameterId = Chunk.AllocateParameter();

            try
            {
                CurrentScope.DefineParameter(
                    parameter.Identifier.Name,
                    parameterId,
                    parameter.ReadWrite,
                    parameter.Line,
                    parameter.Column
                );
            }
            catch (DuplicateSymbolException dse)
            {
                Log.TerminateWithFatal(
                    CompilerDiagnostics.DuplicateSymbolInScope.Create(
                        GetLocation(parameter.Identifier),
                        [new RelatedLocation(
                            new SourceLocation(CurrentFileName, dse.Line, dse.Column),
                            "Previously defined here"
                        )],
                        parameter.Identifier.Name,
                        dse.ExistingSymbol.TypeName,
                        dse.Line,
                        dse.Column
                    ),
                    dse
                );

                // Return just in case - TerminateWithFatal should never return, ever.
                return;
            }

            Chunk.DebugDatabase.SetParameterName(
                parameterId,
                parameter.Identifier.Name,
                parameter.ReadWrite
            );

            if (parameter.Initializer != null)
            {
                Chunk.InitializeParameter(
                    parameterId,
                    ExtractConstantValueFrom(parameter.Initializer)
                );
            }
        }
    }
}
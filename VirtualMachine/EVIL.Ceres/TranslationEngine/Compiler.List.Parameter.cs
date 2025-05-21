namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Miscellaneous;

public partial class Compiler
{
    protected override void Visit(ParameterList parameterList)
    {
        if (parameterList.HasSelf)
        {
            if (Chunk.IsSelfAware)
            {
                Log.EmitWarning(
                    "An explicit `self' parameter is not required for this function.",
                    CurrentFileName,
                    EvilMessageCode.RedundantSelfParameter,
                    parameterList.Line,
                    parameterList.Column
                );
            }
            else
            {
                Chunk.AllocateParameter();
                Chunk.MarkSelfAware();
            }
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
                    $"The symbol '{parameter.Identifier.Name}' already exists in this scope " +
                    $"and is a {dse.ExistingSymbol.TypeName} (previously defined on line {dse.Line}, column {dse.Column}).",
                    CurrentFileName,
                    EvilMessageCode.DuplicateSymbolInScope,
                    parameter.Identifier.Line,
                    parameter.Identifier.Column,
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
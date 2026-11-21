namespace EVIL.Ceres.TranslationEngine;

using EVIL.CommonTypes.Diagnostics;

public partial class Compiler
{
    private void ThrowIfVarReadOnly(string identifier)
    {
        if (!CurrentScope.IsSymbolWriteable(identifier, out var result))
        {
            var sym = result!.Value.Symbol;
                
            Log.TerminateWithFatal(
                CompilerDiagnostics.AttemptToWriteReadOnly.Create(
                    GetLocation(Line, Column),
                    [new RelatedLocation(new SourceLocation(CurrentFileName, sym.DefinedOnLine, sym.DefinedOnColumn), "Defined here as read-only")],
                    sym.Type.ToString().ToLower(),
                    sym.Name,
                    sym.DefinedOnLine
                )
            );
        }
    }
}
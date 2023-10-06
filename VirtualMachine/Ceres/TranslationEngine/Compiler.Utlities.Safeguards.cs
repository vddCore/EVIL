using Ceres.TranslationEngine.Diagnostics;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        private void ThrowIfVarReadOnly(string identifier)
        {
            if (!CurrentScope.IsSymbolWriteable(identifier, out var result))
            {
                var sym = result!.Value.Symbol;
                
                Log.TerminateWithFatal(
                    $"Attempt to set a value of a read-only {sym.Type.ToString().ToLower()} `{sym.Name}' " +
                    $"(defined on line {sym.DefinedOnLine})",
                    CurrentFileName,
                    EvilMessageCode.AttemptToWriteReadOnlyLocal,
                    Line,
                    Column
                );
            }
        }
    }
}
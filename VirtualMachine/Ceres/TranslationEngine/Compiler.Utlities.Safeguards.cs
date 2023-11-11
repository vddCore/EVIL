using Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        private void ThrowIfValReadOnly(string identifier)
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

        private void ThrowIfAssigningNilToNilRejectingSymbol(
            SymbolReferenceExpression symRef,
            Expression expression)
        {
            if (!CurrentScope.IsSymbolNilAccepting(symRef.Identifier, out var result))
            {
                var sym = result!.Value.Symbol;

                if (expression.CanBeNil)
                {
                    Log.TerminateWithFatal(
                        $"Attempt to assign a possible nil value to a nil-rejecting {sym.Type.ToString().ToLower()} `{sym.Name}' " +
                        $"(defined on line {sym.DefinedOnLine})",
                        CurrentFileName,
                        EvilMessageCode.AttemptToWriteNilToNilRejectingLocal,
                        Line,
                        Column
                    );
                }
                
                if (expression is SymbolReferenceExpression assignedSymRef)
                {
                    var isAssignedSymbolNilAccepting = CurrentScope.IsSymbolNilAccepting(
                        assignedSymRef.Identifier,
                        out var assignedResult
                    );

                    var assignedType = "global";
                    if (assignedResult != null)
                    {
                        assignedType = assignedResult.Value.Symbol.Type.ToString().ToLower();
                    }

                    if (isAssignedSymbolNilAccepting)
                    {
                        Log.TerminateWithFatal(
                            $"Attempt to assign the value of a nil-accepting {assignedType} " +
                            $"to a nil-rejecting {sym.Type.ToString().ToLower()} `{sym.Name}` " +
                            $"(defined on line {sym.DefinedOnLine}",
                            CurrentFileName,
                            EvilMessageCode.AttemptToWriteNilToNilRejectingLocal,
                            Line,
                            Column
                        );
                    }
                }
                else if (expression is IndexerExpression)
                {
                    Log.TerminateWithFatal(
                        $"Attempt to assign an indexed value that may evaluate to nil to a nil-rejecting " +
                        $"{sym.Type.ToString().ToLower()} `{sym.Name}` " +
                        $"(defined on line {sym.DefinedOnLine}",
                        CurrentFileName,
                        EvilMessageCode.AttemptToWriteNilToNilRejectingLocal,
                        Line,
                        Column
                    );
                }
            }
        }
    }
}
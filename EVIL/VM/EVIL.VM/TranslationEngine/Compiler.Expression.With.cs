namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.VirtualMachine.ExecutionEngine.Diagnostics;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(WithExpression withExpression)
    {
        Visit(withExpression.BaseExpression);
        Chunk.CodeGenerator.Emit(OpCode.TABCLN, /* isDeepCopy: */ false);

        if (!withExpression.TableExpansionExpression.Keyed)
        {
            Log.TerminateWithFatal(
                CompilerDiagnostics.KeyedExpansionTableExpected.Create(
                    GetLocation(withExpression.TableExpansionExpression)
                )
            );

            return;
        }

        var initializers = withExpression.TableExpansionExpression.Initializers;
        for (var i = 0; i < initializers.Count; i++)
        {
            if (initializers[i] is TableSpreadExpression spread)
            {
                Visit(spread);
            }
            else if (initializers[i] is KeyValuePairExpression kvp)
            {
                Visit(kvp.ValueNode);
                Visit(kvp.KeyNode);
                
                Chunk.CodeGenerator.Emit(OpCode.ELINIT);
            }
        }
    }
}
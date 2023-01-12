using System;
using System.Linq;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override void Visit(EachStatement eachStatement)
        {
            Environment.EnterScope();
            {
                Visit(eachStatement.KeyDefinition);
                Visit(eachStatement.ValueDefinition);

                var keyValue = Environment.LocalScope.Members[eachStatement.KeyDefinition.Identifier];
                var valueValue = Environment.LocalScope.Members[eachStatement.ValueDefinition.Identifier];
                
                try
                {
                    var loopFrame = new LoopFrame();
                    Environment.LoopStack.Push(loopFrame);

                    var tableValue = Visit(eachStatement.Iterable);

                    if (tableValue.Type != DynValueType.Table)
                    {
                        throw new RuntimeException(
                            $"Expected a Table, got {tableValue.Type}.",
                            Environment,
                            eachStatement.Iterable.Line
                        );
                    }

                    var actualTable = tableValue.Table;

                    actualTable.ForEach((k, v) =>
                    {
                        keyValue.CopyFrom(k);
                        valueValue.CopyFrom(v);

                        Visit(eachStatement.Body);
                    });
                }
                finally
                {
                    Environment.LoopStack.Pop();
                }
            }
            Environment.ExitScope();
        }
    }
}
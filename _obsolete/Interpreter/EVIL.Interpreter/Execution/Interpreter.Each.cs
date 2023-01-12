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
                Visit(eachStatement.Initialization);

                var localScope = Environment.LocalScope.Members;
                var definitions = eachStatement.Initialization.Definitions;

                DynValue keyValue = null;
                DynValue valueValue;

                if (definitions.Count == 1)
                {
                    valueValue = localScope[definitions.Keys.ElementAt(0)];
                }
                else if (definitions.Count == 2)
                {
                    keyValue = localScope[definitions.Keys.ElementAt(0)];
                    valueValue = localScope[definitions.Keys.ElementAt(1)];
                }
                else
                {
                    throw new RuntimeException(
                        $"Expected at least 1 and at most 2 loop variables, found {definitions.Count}.",
                        this,
                        eachStatement.Line
                    );
                }

                try
                {
                    var loopFrame = new LoopFrame();
                    CallStack.Peek().LoopStack.Push(loopFrame);

                    var tableValue = Visit(eachStatement.Iterable);

                    if (tableValue.Type != DynValueType.Table)
                    {
                        throw new RuntimeException(
                            $"Expected a Table, got {tableValue.Type}.",
                            this,
                            eachStatement.Iterable.Line
                        );
                    }

                    var actualTable = tableValue.Table;

                    actualTable.ForEach((k, v) =>
                    {
                        if (keyValue != null)
                            keyValue.CopyFrom(k);

                        valueValue.CopyFrom(v);

                        Visit(eachStatement.Body);
                    });
                }
                finally
                {
                    CallStack.Peek().LoopStack.Pop();
                }
            }
            Environment.ExitScope();
        }
    }
}
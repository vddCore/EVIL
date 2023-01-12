using System;
using EVIL.Abstraction;
using EVIL.AST.Nodes;
using EVIL.Diagnostics;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(EachLoopNode eachLoopNode)
        {
            var keyName = eachLoopNode.KeyNode.Name;
            var valueName = eachLoopNode.ValueNode.Name;

            try
            {
                var lsItem = new LoopFrame();
                Environment.LoopStack.Push(lsItem);

                var tableValue = Visit(eachLoopNode.TableNode);

                if (tableValue.Type != DynValueType.Table)
                    throw new RuntimeException($"Expected a table, got {tableValue.Type.ToString().ToLower()}.",
                        eachLoopNode.TableNode.Line);

                var actualTable = tableValue.Table;

                try
                {
                    Environment.EnterScope();
                    foreach (var element in actualTable)
                    {
                        Environment.LocalScope.Set(keyName, element.Key);
                        Environment.LocalScope.Set(valueName, element.Value);

                        ExecuteStatementList(eachLoopNode.StatementList);
                    }
                }
                catch (InvalidOperationException)
                {
                    throw new RuntimeException("The table was modified, cannot continue execution.", eachLoopNode.Line);
                }
                finally
                {
                    Environment.ExitScope();
                }
            }
            finally
            {
                Environment.LoopStack.Pop();
            }

            return DynValue.Zero;
        }
    }
}
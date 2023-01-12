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

            DynValue backupKeyVar = null;
            DynValue backupValueVar = null;

            try
            {
                var lsItem = new LoopStackItem();
                LoopStack.Push(lsItem);

                DynValue tableValue = null;
                if (eachLoopNode.TableNode is VariableNode varNode)
                {
                    var tableName = varNode.Name;
                    if (CallStack.Count > 0)
                    {
                        var csItem = CallStack.Peek();

                        if (csItem.LocalVariableScope.ContainsKey(tableName))
                            tableValue = csItem.LocalVariableScope[tableName];
                        else if (csItem.ParameterScope.ContainsKey(tableName))
                            tableValue = csItem.ParameterScope[tableName];
                    }

                    if (tableValue == null)
                    {
                        if (Environment.Globals.ContainsKey(tableName))
                            tableValue = Environment.Globals[tableName];
                        else
                            throw new RuntimeException($"Variable '{tableName}' could not be found in any known scope.",
                                eachLoopNode.TableNode.Line);
                    }
                }
                else if (eachLoopNode.TableNode is FunctionCallNode fnCallNode)
                {
                    tableValue = Visit(fnCallNode);
                }
                else
                {
                    throw new RuntimeException("Expected a variable or function call.", eachLoopNode.TableNode.Line);
                }

                if (tableValue.Type != DynValueType.Table)
                    throw new RuntimeException($"Expected a table, got {tableValue.Type.ToString().ToLower()}.",
                        eachLoopNode.TableNode.Line);

                var actualTable = tableValue.Table;

                if (Environment.Globals.ContainsKey(keyName))
                    backupKeyVar = Environment.Globals[keyName];

                if (Environment.Globals.ContainsKey(valueName))
                    backupValueVar = Environment.Globals[valueName];

                try
                {
                    foreach (var element in actualTable)
                    {
                        if (Environment.Globals.ContainsKey(keyName))
                            Environment.Globals.Remove(keyName);

                        if (Environment.Globals.ContainsKey(valueName))
                            Environment.Globals.Remove(valueName);

                        Environment.Globals.Add(keyName, element.Key);
                        Environment.Globals.Add(valueName, element.Value);

                        ExecuteStatementList(eachLoopNode.StatementList);
                    }
                }
                catch (InvalidOperationException)
                {
                    throw new RuntimeException("The table was modified, cannot continue execution.", eachLoopNode.Line);
                }
                finally
                {
                    Environment.Globals.Remove(keyName);
                    Environment.Globals.Remove(valueName);
                }
            }
            finally
            {
                if (backupKeyVar != null)
                    Environment.Globals.Add(keyName, backupKeyVar);

                if (backupValueVar != null)
                    Environment.Globals.Add(valueName, backupValueVar);

                LoopStack.Pop();
            }

            return DynValue.Zero;
        }
    }
}
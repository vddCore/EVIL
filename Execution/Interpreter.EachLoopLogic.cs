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
            var keyName = eachLoopNode.KeyNode.Identifier;
            var valueName = eachLoopNode.ValueNode.Identifier;

            Environment.EnterScope();
            {
                try
                {
                    var loopFrame = new LoopFrame();
                    Environment.LoopStack.Push(loopFrame);

                    var tableValue = Visit(eachLoopNode.TableNode);

                    if (tableValue.Type != DynValueType.Table)
                        throw new RuntimeException($"Expected a table, got {tableValue.Type.ToString().ToLower()}.",
                            eachLoopNode.TableNode.Line);

                    var actualTable = tableValue.Table;

                    try
                    {
                        foreach (var element in actualTable)
                        {
                            Environment.LocalScope.Set(keyName, element.Key);
                            Environment.LocalScope.Set(valueName, element.Value);

                            ExecuteStatementList(eachLoopNode.StatementList);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        throw new RuntimeException("The table was modified, cannot continue execution.",
                            eachLoopNode.Line);
                    }
                }
                finally
                {
                    Environment.LoopStack.Pop();
                }
            }
            Environment.ExitScope();
            
            return DynValue.Zero;
        }
    }
}
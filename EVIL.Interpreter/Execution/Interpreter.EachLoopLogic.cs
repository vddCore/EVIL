using System;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(EachLoopNode eachLoopNode)
        {
            Environment.EnterScope();
            {
                var keyValue = Visit(eachLoopNode.KeyNode);
                var valueValue = Visit(eachLoopNode.ValueNode);
                
                try
                {
                    var loopFrame = new LoopFrame();
                    Environment.LoopStack.Push(loopFrame);

                    var tableValue = Visit(eachLoopNode.TableNode);

                    if (tableValue.Type != DynValueType.Table)
                        throw new RuntimeException($"Expected a Table, got {tableValue.Type}.",
                            eachLoopNode.TableNode.Line);

                    var actualTable = tableValue.Table;

                    try
                    {
                        foreach (var element in actualTable)
                        {
                            keyValue.CopyFrom(element.Key, true);
                            valueValue.CopyFrom(element.Value, true);

                            Environment.EnterScope();
                            {
                                ExecuteStatementList(eachLoopNode.StatementList);
                            }
                            Environment.ExitScope();
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
using System;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(FunctionCallNode functionCallNode)
        {
            var invokable = Visit(functionCallNode.Left);

            if (invokable.Type == DynValueType.Function)
            {
                return InvokeFunction(functionCallNode, invokable);
            }
            else if (invokable.Type == DynValueType.ClrFunction)
            {
                return InvokeFunction(functionCallNode, invokable);
            }
            else if (invokable.Type == DynValueType.Table)
            {
                return InitTable(functionCallNode, invokable);
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt to invoke an un-invokable value {invokable.Type}.",
                    functionCallNode.Line
                );
            }
        }

        private DynValue InvokeFunction(FunctionCallNode functionCallNode, DynValue funcValue)
        {
            var funcName = "<anonymous>";

            if (functionCallNode.Left is VariableNode vn)
            {
                funcName = vn.Identifier;
                funcValue = Environment.LocalScope.FindInScopeChain(vn.Identifier);

                if (funcValue == null)
                {
                    throw new RuntimeException(
                        $"'{funcName}' was not found in the current scope.", functionCallNode.Line
                    );
                }
            }

            var parameters = new ClrFunctionArguments();

            foreach (var node in functionCallNode.Parameters)
                parameters.Add(Visit(node));

            if (Environment.CallStack.Count > Environment.CallStackLimit)
            {
                throw new RuntimeException("Call stack overflow.", functionCallNode.Line);
            }

            DynValue retVal;
            if (funcValue.Type == DynValueType.ClrFunction)
            {
                retVal = ExecuteClrFunction(funcValue.ClrFunction, funcName, parameters);
            }
            else
            {
                Environment.EnterScope(Environment.GlobalScope.HasMember(funcName));
                {
                    retVal = ExecuteScriptFunction(funcValue.ScriptFunction, funcName, parameters, functionCallNode);
                }
                Environment.ExitScope();
            }

            return retVal;
        }

        private DynValue InitTable(FunctionCallNode functionCallNode, DynValue tableValue)
        {
            if (functionCallNode.Parameters.Count == 1)
            {
                var count = Visit(functionCallNode.Parameters[0]);

                if (count.Type != DynValueType.Number || count.Number % 1 != 0)
                {
                    throw new RuntimeException("Table initializer must be an integer.", functionCallNode.Line);
                }

                tableValue.Table.Clear();
                for (var i = 0; i < (int)count.Number; i++)
                {
                    tableValue.Table[i] = DynValue.Zero;
                }
            }
            else if (functionCallNode.Parameters.Count == 2)
            {
                var count = Visit(functionCallNode.Parameters[0]);
                if (count.Type != DynValueType.Number || count.Number % 1 != 0)
                {
                    throw new RuntimeException("Table initializer must be an integer.", functionCallNode.Line);
                }

                var value = Visit(functionCallNode.Parameters[1]);
                for(var i = 0; i < (int)count.Number; i++)
                {
                    tableValue.Table[i] = value.Copy();
                }
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt to initialize a table using {functionCallNode.Parameters.Count} parameters.",
                    functionCallNode.Line
                );
            }

            return tableValue;
        }

        private DynValue ExecuteScriptFunction(ScriptFunction scriptFunction, string name, ClrFunctionArguments args, FunctionCallNode node)
        {
            var callStackItem = new StackFrame(name);
            var iterator = 0;
            
            foreach (var closure in scriptFunction.Closures)
            {
                Environment.LocalScope.Set(closure.Key, closure.Value);
            }

            foreach (var parameterName in scriptFunction.ParameterNames)
            {
                if (Environment.LocalScope.HasMember(parameterName))
                    throw new RuntimeException($"Duplicate parameter name '{parameterName}'.", null);

                if (iterator < args.Count)
                    Environment.LocalScope.Set(parameterName, args[iterator++]);
                else
                    Environment.LocalScope.Set(parameterName, DynValue.Zero);
            }

            Environment.CallStack.Push(callStackItem);

            var retval = DynValue.Zero;
            try
            {
                retval = ExecuteStatementList(scriptFunction.StatementList);
            }
            catch (RuntimeException)
            {
                throw;
            }
            catch (ExitStatementException)
            {
            }
            catch (Exception e)
            {
                throw new RuntimeException(e.Message, node.Line);
            }
            finally
            {
                Environment.CallStack.Pop();
            }

            return retval;
        }

        private DynValue ExecuteClrFunction(ClrFunction clrFunction, string name, ClrFunctionArguments args)
        {
            var csi = new StackFrame($"CLR!{name}");
            Environment.CallStack.Push(csi);

            DynValue retVal;
            try
            {
                retVal = clrFunction.Invokable(this, args);
            }
            finally
            {
                Environment.CallStack.Pop();
            }

            return retVal;
        }
    }
}
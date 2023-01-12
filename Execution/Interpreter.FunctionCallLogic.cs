using System;
using EVIL.Abstraction;
using EVIL.Abstraction.Base;
using EVIL.AST.Nodes;
using EVIL.Diagnostics;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(FunctionCallNode functionCallNode)
        {
            var name = functionCallNode.MemberName;
            var funcValue = Environment.LocalScope.FindInScopeChain(name);

            if (funcValue == null)
            {
                throw new RuntimeException(
                    $"'{name} was not found in the current scope.", functionCallNode.Line
                );
            }

            if (funcValue.Type != DynValueType.Function)
            {
                throw new RuntimeException(
                    $"'{name}' cannot be invoked because it is not a function.", functionCallNode.Line
                );
            }

            var parameters = new ClrFunctionArguments();

            foreach (var node in functionCallNode.Parameters)
                parameters.Add(Visit(node));

            if (Environment.CallStack.Count > Environment.CallStackLimit)
            {
                throw new RuntimeException("Call stack overflow.", functionCallNode.Line);
            }

            if (funcValue.IsClrFunction)
            {
                return ExecuteClrFunction(funcValue.ClrFunction, name, parameters);
            }
            else
            {
                return ExecuteScriptFunction(funcValue.ScriptFunction, parameters, name);
            }
        }

        private DynValue ExecuteScriptFunction(ScriptFunction scriptFunction, ClrFunctionArguments args, string name)
        {
            var callStackItem = new CallStackItem(name);
            var iterator = 0;
            foreach (var parameterName in scriptFunction.ParameterNames)
            {
                if (callStackItem.Parameters.ContainsKey(parameterName))
                    throw new RuntimeException($"Duplicate parameter name '{parameterName}'.", null);

                if (iterator < args.Count)
                    callStackItem.Parameters.Add(parameterName, args[iterator++]);
                else
                    callStackItem.Parameters.Add(parameterName, DynValue.Zero);
            }

            Environment.CallStack.Push(callStackItem);

            var retval = DynValue.Zero;
            try
            {
                Environment.EnterScope();
                retval = ExecuteStatementList(scriptFunction.StatementList);
            }
            catch (ProgramTerminationException)
            {
                throw;
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
                throw new RuntimeException(e.Message, null);
            }
            finally
            {
                Environment.ExitScope();
                Environment.CallStack.Pop();
            }

            return retval;
        }

        private DynValue ExecuteClrFunction(IFunction function, string name, ClrFunctionArguments args)
        {
            var clrFunction = function as ClrFunction;

            if (clrFunction == null)
                throw new RuntimeException("Failed to interpret IFunction as ClrFunction.", null);

            var csi = new CallStackItem($"CLR!{name}");
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
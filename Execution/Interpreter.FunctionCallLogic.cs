using System;
using EVIL.Abstraction;
using EVIL.AST.Nodes;
using EVIL.Diagnostics;

namespace EVIL.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(FunctionCallNode functionCallNode)
        {
            var name = functionCallNode.FunctionName;
            var parameters = new ClrFunctionArguments();

            foreach (var node in functionCallNode.Parameters)
                parameters.Add(Visit(node));

            if (Environment.Functions.ContainsKey(name))
            {
                var scriptFunction = Environment.Functions[name];

                if (CallStack.Count > 255)
                {
                    while (CallStack.Count != 0)
                        CallStack.Pop();

                    throw new RuntimeException("Call stack overflow.", functionCallNode.Line);
                }

                var callStackItem = new CallStackItem(name);
                var iterator = 0;
                foreach (var parameterName in scriptFunction.ParameterNames)
                {
                    if (callStackItem.ParameterScope.ContainsKey(parameterName))
                        throw new RuntimeException($"Parameter {parameterName} already defined.", functionCallNode.Line);

                    if (iterator < parameters.Count)
                        callStackItem.ParameterScope.Add(parameterName, parameters[iterator++]);
                    else
                        callStackItem.ParameterScope.Add(parameterName, DynValue.Zero);
                }

                CallStack.Push(callStackItem);

                var retval = DynValue.Zero;
                try
                {
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
                catch (ExitStatementException) { }
                catch (Exception e)
                {
                    throw new RuntimeException(e.Message, functionCallNode.Line);
                }
                finally
                {
                    CallStack.Pop();
                }

                return retval;
            }
            else if (Environment.BuiltIns.ContainsKey(name))
            {
                var clrFunction = Environment.BuiltIns[name];

                if (CallStack.Count > 255)
                {
                    while (CallStack.Count != 0)
                        CallStack.Pop();

                    throw new RuntimeException("Call stack overflow.", functionCallNode.Line);
                }

                var callStackitem = new CallStackItem($"kernel![{name}]");

                CallStack.Push(callStackitem);

                DynValue retVal;
                try
                {
                    retVal = clrFunction(this, parameters);
                }
                finally
                {
                    CallStack.Pop();
                }

                return retVal;
            }
            else throw new RuntimeException($"Function '{name}' does not exist.", functionCallNode.Line);
        }
    }
}

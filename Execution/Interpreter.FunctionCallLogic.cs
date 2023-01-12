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
            var name = functionCallNode.MemberName;
            var parameters = new ClrFunctionArguments();

            foreach (var node in functionCallNode.Parameters)
                parameters.Add(Visit(node));

            if (Environment.Functions.ContainsKey(name))
            {
                var scriptFunction = Environment.Functions[name];

                if (CallStack.Count > CallStackLimit)
                {
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

                if (CallStack.Count > CallStackLimit)
                {
                    CallStack.Pop();
                    throw new RuntimeException("Call stack overflow.", functionCallNode.Line);
                }

                var callStackitem = new CallStackItem($"CLR!{name}");

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
            else if (Environment.Globals.ContainsKey(name))
            {
                var scriptFunction = Environment.Globals[name].Function;

                if (CallStack.Count > CallStackLimit)
                {
                    CallStack.Pop();
                    throw new RuntimeException("Call stack overflow.", functionCallNode.Line);
                }

                var callStackItem = new CallStackItem(name);
                var iterator = 0;
                foreach (var parameterName in scriptFunction.ParameterNames)
                {
                    if (callStackItem.ParameterScope.ContainsKey(parameterName))
                        throw new RuntimeException($"Duplicate parameter name '{parameterName}'.", functionCallNode.Line);

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
            else if (CallStack.Count > 0)
            {
                var csi = CallStack.Peek();
                var scriptFunction = csi.LocalVariableScope[name].Function;

                if (CallStack.Count > CallStackLimit)
                {
                    CallStack.Pop();
                    throw new RuntimeException("Call stack overflow.", functionCallNode.Line);
                }

                var callStackItem = new CallStackItem("LOCAL!" + name);
                var iterator = 0;
                foreach (var parameterName in scriptFunction.ParameterNames)
                {
                    if (callStackItem.ParameterScope.ContainsKey(parameterName))
                        throw new RuntimeException($"Duplicate parameter name '{parameterName}'.", functionCallNode.Line);

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
            else throw new RuntimeException($"'{name}' is not a top-level nor a variable function.", functionCallNode.Line);
        }

        private void EmptyStack()
        {
            while (CallStack.Count != 0)
                CallStack.Pop();
        }
    }
}

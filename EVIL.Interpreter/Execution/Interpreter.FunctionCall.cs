using System;
using System.Collections.Generic;
using EVIL.Grammar.AST;
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
            else
            {
                throw new RuntimeException(
                    $"Attempt to invoke an un-invokable value {invokable.Type}.",
                    Environment,
                    functionCallNode.Line
                );
            }
        }

        private DynValue InvokeFunction(FunctionCallNode functionCallNode, DynValue funcValue)
        {
            if (Environment.CallStack.Count > Environment.CallStackLimit)
            {
                throw new RuntimeException(
                    "Call stack overflow.",
                    Environment,
                    functionCallNode.Line
                );
            }

            var funcName = "<anonymous>";
            var parameters = new FunctionArguments();

            if (functionCallNode.Left is IndexingNode indexingNode)
            {
                funcName = indexingNode.BuildChainStringRepresentation();
            }
            else if (functionCallNode.Left is VariableNode variableNode)
            {
                funcName = variableNode.Identifier;
            }

            foreach (var node in functionCallNode.Parameters)
                parameters.Add(Visit(node));

            DynValue retVal;
            if (funcValue.Type == DynValueType.ClrFunction)
            {
                retVal = ExecuteClrFunction(funcValue.ClrFunction, funcName, parameters, functionCallNode);
            }
            else
            {
                Environment.EnterScope(true);
                {
                    retVal = ExecuteScriptFunction(funcValue.ScriptFunction, funcName, parameters, functionCallNode);
                }
                Environment.ExitScope();
            }

            return retVal;
        }

        private DynValue ExecuteScriptFunction(ScriptFunction scriptFunction, string name, FunctionArguments args,
            AstNode node)
        {
            var callStackItem = new StackFrame(name, scriptFunction.ParameterNames)
            {
                InvokedAtLine = node.Line,
                DefinedAtLine = scriptFunction.DefinedAtLine
            };

            var iterator = 0;

            foreach (var closure in scriptFunction.Closures)
            {
                Environment.LocalScope.Set(closure.Key, closure.Value);
            }

            foreach (var parameterName in scriptFunction.ParameterNames)
            {
                if (Environment.LocalScope.HasMember(parameterName))
                {
                    throw new RuntimeException(
                        $"Duplicate parameter name '{parameterName}'.",
                        Environment,
                        null
                    );
                }

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
                throw new RuntimeException(
                    e.Message,
                    Environment,
                    node.Line
                );
            }
            finally
            {
                Environment.CallStack.Pop();
            }

            return retval;
        }

        private DynValue ExecuteClrFunction(ClrFunction clrFunction, string name, FunctionArguments args, AstNode node)
        {
            var parameterList = new List<string>();
            var parameters = clrFunction.Invokable.Method.GetParameters();

            for (var i = 0; i < parameters.Length; i++)
            {
                parameterList.Add(parameters[i].Name);
            }
            
            var frame = new StackFrame(
                $"CLR!<{name}>", 
                parameterList
            ) {InvokedAtLine = node.Line};

            Environment.CallStack.Push(frame);

            DynValue retval;
            try
            {
                retval = clrFunction.Invokable(this, args);
            }
            catch (ClrFunctionException e)
            {
                throw new RuntimeException(e.Message, Environment, node.Line, e);
            }
            finally
            {
                Environment.CallStack.Pop();
            }

            return retval;
        }
    }
}
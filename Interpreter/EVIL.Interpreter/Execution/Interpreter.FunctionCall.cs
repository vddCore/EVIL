using System;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(FunctionCallExpression functionCallExpression)
        {
            var invokable = Visit(functionCallExpression.Callee);

            if (invokable.Type == DynValueType.Function)
            {
                return InvokeFunction(functionCallExpression, invokable);
            }
            else if (invokable.Type == DynValueType.ClrFunction)
            {
                return InvokeFunction(functionCallExpression, invokable);
            }
            else
            {
                throw new RuntimeException(
                    $"Attempt to invoke an un-invokable value {invokable.Type}.",
                    Environment,
                    functionCallExpression.Line
                );
            }
        }

        private DynValue InvokeFunction(FunctionCallExpression functionCallExpression, DynValue funcValue)
        {
            if (Environment.CallStack.Count > Environment.CallStackLimit)
            {
                throw new RuntimeException(
                    "Call stack overflow.",
                    Environment,
                    functionCallExpression.Line
                );
            }

            var funcName = "<anonymous>";

            if (functionCallExpression.Callee is IndexerExpression indexingNode)
            {
                funcName = indexingNode.BuildChainStringRepresentation();
            }
            else if (functionCallExpression.Callee is VariableReferenceExpression variableNode)
            {
                funcName = variableNode.Identifier;
            }

            var args = new FunctionArguments();

            for (var i = 0; i < functionCallExpression.Arguments.Count; i++)
            {
                args.Add(Visit(functionCallExpression.Arguments[i]));
            }

            DynValue retVal;
            if (funcValue.Type == DynValueType.ClrFunction)
            {
                retVal = ExecuteClrFunction(funcValue.ClrFunction, funcName, args, functionCallExpression);
            }
            else
            {
                retVal = ExecuteScriptFunction(funcValue.ScriptFunction, funcName, args, functionCallExpression);
            }

            return retVal;
        }

        private DynValue ExecuteScriptFunction(ScriptFunction scriptFunction, string name, FunctionArguments args,
            AstNode node)
        {
            var scope = new NameScope(Environment, null);
            var stackFrame = new StackFrame(name)
            {
                InvokedAtLine = node.Line,
                DefinedAtLine = scriptFunction.DefinedAtLine,
                PreviousScope = Environment.LocalScope,
            };

            var iterator = 0;

            foreach (var closure in scriptFunction.Closures)
            {
                scope.Set(closure.Key, closure.Value);
            }

            for (var i = 0; i < scriptFunction.Parameters.Count; i++)
            {
                var parameterName = scriptFunction.Parameters[i];

                if (scope.HasMember(parameterName))
                {
                    throw new RuntimeException(
                        $"'{parameterName}' was already declared in this scope.",
                        Environment,
                        node.Line
                    );
                }

                if (iterator < args.Count)
                    scope.Set(parameterName, args[iterator++]);
                else
                    scope.Set(parameterName, DynValue.Zero);
            }

            stackFrame.Parameters.AddRange(scriptFunction.Parameters);
            stackFrame.Arguments.AddRange(args);

            Environment.CallStack.Push(stackFrame);

            DynValue retVal;

            try
            {
                Environment.LocalScope = scope;
                Visit(scriptFunction.Statements);
                retVal = Environment.CallStack.Peek().ReturnValue;
            }
            catch (RuntimeException)
            {
                throw;
            }
            catch (ExitStatementException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new RuntimeException(
                    e.Message,
                    Environment,
                    node.Line,
                    e
                );
            }
            finally
            {
                Environment.LocalScope = Environment.StackTop.PreviousScope;
                Environment.CallStack.Pop();
            }

            return retVal;
        }

        private DynValue ExecuteClrFunction(ClrFunction clrFunction, string name, FunctionArguments args,
            AstNode node)
        {
            var parameters = clrFunction.Invokable.Method.GetParameters();

            var frame = new StackFrame(
                $"CLR!<{name}>"
            ) { InvokedAtLine = node.Line };

            for (var i = 0; i < parameters.Length; i++)
            {
                frame.Parameters.Add(parameters[i].Name);
            }

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
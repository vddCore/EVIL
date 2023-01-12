﻿using EVIL.Grammar.AST.Nodes;
using EVIL.Interpreter.Abstraction;

namespace EVIL.Interpreter.Execution
{
    public partial class Interpreter
    {
        public override DynValue Visit(VariableReference variableReference)
        {
            var dynValue = Environment.LocalScope?.FindInScope(variableReference.Identifier)
                           ?? Environment.GlobalScope.FindInScope(variableReference.Identifier);

            if (dynValue == null)
            {
                throw new RuntimeException(
                    $"'{variableReference.Identifier}' does not exist in the current scope.",
                    Environment,
                    variableReference.Line
                );
            }

            return dynValue;
        }
    }
}
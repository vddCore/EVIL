using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Interpreter.Abstraction
{
    public class ScriptFunction
    {
        public Dictionary<string, DynValue> Closures { get; } = new();

        public BlockStatementNode Statements { get; }
        public List<string> ParameterNames { get; }
        public int DefinedAtLine { get; }

        public ScriptFunction(BlockStatementNode statements, List<string> parameterNames, int definedAtLine)
        {
            Statements = statements;
            ParameterNames = parameterNames;
            DefinedAtLine = definedAtLine;
        }
    }
}
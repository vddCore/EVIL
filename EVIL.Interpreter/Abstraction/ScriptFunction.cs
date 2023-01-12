using System.Collections.Generic;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Interpreter.Abstraction
{
    public class ScriptFunction
    {
        public Dictionary<string, DynValue> Closures { get; } = new();

        public BlockStatementNode Statements { get; }
        public ParameterListNode Parameters { get; }
        
        public int DefinedAtLine { get; }

        public ScriptFunction(BlockStatementNode statements,ParameterListNode parameters, int definedAtLine)
        {
            Statements = statements;
            Parameters = parameters;
            
            DefinedAtLine = definedAtLine;
        }
    }
}
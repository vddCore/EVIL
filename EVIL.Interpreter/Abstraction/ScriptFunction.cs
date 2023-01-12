using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;

namespace EVIL.Interpreter.Abstraction
{
    public class ScriptFunction
    {
        public Dictionary<string, DynValue> Closures { get; } = new();

        public BlockStatement Statements { get; }
        public List<string> Parameters { get; }
        
        public int DefinedAtLine { get; }

        public ScriptFunction(BlockStatement statements, List<string> parameters, int definedAtLine)
        {
            Statements = statements;
            Parameters = parameters;
            
            DefinedAtLine = definedAtLine;
        }
    }
}
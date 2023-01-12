using System.Collections.Generic;
using EVIL.Grammar.AST;

namespace EVIL.Interpreter.Abstraction
{
    public class ScriptFunction
    {
        public Dictionary<string, DynValue> Closures { get; } = new();

        public List<AstNode> StatementList { get; }
        public List<string> ParameterNames { get; }
        public int DefinedAtLine { get; }
        public bool IsConstructor { get; }

        public ScriptFunction(List<AstNode> statementList, List<string> parameterNames, int definedAtLine,
            bool isConstructor = false)
        {
            StatementList = statementList;
            ParameterNames = parameterNames;
            DefinedAtLine = definedAtLine;
            IsConstructor = isConstructor;
        }
    }
}
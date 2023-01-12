using System.Collections.Generic;
using EVIL.AST.Base;

namespace EVIL.Abstraction
{
    public class ScriptFunction
    {
        public Dictionary<string, DynValue> Closures { get; } = new();
        public List<AstNode> StatementList { get; }
        public List<string> ParameterNames { get; }

        public ScriptFunction(List<AstNode> statementList, List<string> parameterNames)
        {
            StatementList = statementList;
            ParameterNames = parameterNames;
        }
    }
}
using System.Collections.Generic;
using EVIL.Abstraction.Base;
using EVIL.AST.Base;

namespace EVIL.Abstraction
{
    public class ScriptFunction : IFunction
    {
        public List<AstNode> StatementList { get; }
        public List<string> ParameterNames { get; }

        public ScriptFunction(List<AstNode> statementList, List<string> parameterNames)
        {
            StatementList = statementList;
            ParameterNames = parameterNames;
        }
    }
}
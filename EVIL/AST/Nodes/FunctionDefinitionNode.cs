using System.Collections.Generic;
using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class FunctionDefinitionNode : AstNode
    {
        public string Name { get; }
        public List<AstNode> StatementList { get; }
        public List<string> ParameterNames { get; }

        public FunctionDefinitionNode(string name, List<AstNode> statementList, List<string> parameterNames)
        {
            Name = name;
            StatementList = statementList;
            ParameterNames = parameterNames;
        }
    }
}
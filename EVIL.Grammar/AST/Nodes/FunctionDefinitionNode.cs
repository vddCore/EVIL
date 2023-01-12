using System.Collections.Generic;

namespace EVIL.Grammar.AST.Nodes
{
    public class FunctionDefinitionNode : AstNode
    {
        public string Identifier { get; }
        public List<AstNode> StatementList { get; }
        public List<string> ParameterNames { get; }

        public FunctionDefinitionNode(string identifier, List<AstNode> statementList, List<string> parameterNames)
        {
            Identifier = identifier;
            StatementList = statementList;
            ParameterNames = parameterNames;
        }
    }
}
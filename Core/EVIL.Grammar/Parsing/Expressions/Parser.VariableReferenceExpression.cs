using EVIL.Grammar.AST.Expressions;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private VariableReferenceExpression VariableReference()
        {
            var identifier = Identifier();
            
            return new VariableReferenceExpression(identifier.Name)
                { Line = identifier.Line, Column = identifier.Column };
        }
    }
}
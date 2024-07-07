using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements.TopLevel;

namespace EVIL.Grammar.AST.Statements
{
    public class FnTargetedStatement : Statement
    {
        public AstNode PrimaryTarget { get; }
        public IdentifierNode SecondaryIdentifier { get; }
        public AttributeList? AttributeList { get; }
        public ParameterList? ParameterList { get; }
        public Statement InnerStatement { get; }

        public bool IsSelfTargeting => PrimaryTarget is SelfExpression;
        
        public FnTargetedStatement(
            AstNode primaryTarget,
            IdentifierNode secondaryIdentifier,
            AttributeList? attributeList,
            ParameterList? parameterList,
            Statement innerStatement)
        {
            PrimaryTarget = primaryTarget;
            SecondaryIdentifier = secondaryIdentifier;
            AttributeList = attributeList;
            ParameterList = parameterList;
            InnerStatement = innerStatement;

            Reparent(PrimaryTarget);
            Reparent(SecondaryIdentifier);
            Reparent(AttributeList);
            Reparent(ParameterList);
            Reparent(InnerStatement);
        }
    }
}
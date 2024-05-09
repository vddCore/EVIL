using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements
{
    public class FnTargetedStatement : Statement
    {
        public AstNode PrimaryTarget { get; }
        public IdentifierNode SecondaryIdentifier { get; }
        public ParameterList? ParameterList { get; }
        public Statement Statement { get; }
        public List<AttributeNode> Attributes { get; }

        public bool IsSelfTargeting => PrimaryTarget is SelfExpression;
        
        public FnTargetedStatement(
            AstNode primaryTarget,
            IdentifierNode secondaryIdentifier,
            ParameterList? parameterList,
            Statement statement,
            List<AttributeNode> attributes)
        {
            PrimaryTarget = primaryTarget;
            SecondaryIdentifier = secondaryIdentifier;
            ParameterList = parameterList;
            Statement = statement;
            Attributes = attributes;

            Reparent(PrimaryTarget);
            Reparent(SecondaryIdentifier);
            Reparent(ParameterList);
            Reparent(Statement);
            Reparent(Attributes);
        }
    }
}
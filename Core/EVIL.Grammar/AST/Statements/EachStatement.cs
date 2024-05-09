using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements
{
    public class EachStatement : Statement
    {
        public IdentifierNode KeyIdentifier { get; }
        public IdentifierNode? ValueIdentifier { get; }
        
        public Expression Iterable { get; }
        public Statement Statement { get; }

        public EachStatement(
            IdentifierNode keyIdentifier,
            IdentifierNode? valueIdentifier,
            Expression iterable,
            Statement statement)
        {
            KeyIdentifier = keyIdentifier;
            ValueIdentifier = valueIdentifier;
            Iterable = iterable;
            Statement = statement;

            Reparent(KeyIdentifier);
            Reparent(ValueIdentifier);
            Reparent(Iterable, Statement);
        }
    }
}
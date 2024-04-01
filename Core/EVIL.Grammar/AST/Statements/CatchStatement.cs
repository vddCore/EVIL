using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements
{
    public class CatchStatement : Statement
    {
        public IdentifierNode? ExceptionLocalIdentifier { get; }
        public Statement InnerStatement { get; }

        public CatchStatement(IdentifierNode? exceptionLocalIdentifier, Statement innerStatement)
        {
            ExceptionLocalIdentifier = exceptionLocalIdentifier;
            InnerStatement = innerStatement;

            if (ExceptionLocalIdentifier != null)
            {
                Reparent(ExceptionLocalIdentifier);
            }
            
            Reparent(InnerStatement);
        }
    }
}
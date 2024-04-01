using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

namespace EVIL.Grammar.AST.Statements
{
    public class TryStatement : Statement
    {
        public Statement InnerStatement { get; }

        public IdentifierNode HandlerExceptionLocal { get; }
        public Statement HandlerStatement { get; }

        public TryStatement(Statement innerStatement, IdentifierNode handlerExceptionLocal, Statement handlerStatement)
        {
            InnerStatement = innerStatement;
            HandlerExceptionLocal = handlerExceptionLocal;
            HandlerStatement = handlerStatement;
            
            Reparent(InnerStatement);
            Reparent(HandlerExceptionLocal);
            Reparent(HandlerStatement);
        }
    }
}
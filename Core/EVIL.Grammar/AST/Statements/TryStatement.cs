namespace EVIL.Grammar.AST.Statements;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

public class TryStatement : Statement
{
    public Statement InnerStatement { get; }
    public IdentifierNode? HandlerExceptionLocal { get; }
    public Statement HandlerStatement { get; }

    public TryStatement(
        Statement innerStatement,
        IdentifierNode? handlerExceptionLocal, 
        Statement handlerStatement)
    {
        InnerStatement = innerStatement;
        HandlerExceptionLocal = handlerExceptionLocal;
        HandlerStatement = handlerStatement;
            
        Reparent(InnerStatement, HandlerExceptionLocal, HandlerStatement);
    }
}
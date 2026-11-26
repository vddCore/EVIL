namespace EVIL.Core.Grammar.AST.Miscellaneous;

using System.Collections.Generic;
using System.Linq;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Statements;

public sealed class ProgramNode : AstNode
{
    public IEnumerable<Statement> FnStatements => Statements.Where(x => x is FnStatement);
    public IEnumerable<Statement> AnythingButFnStatements => Statements.Where(x => x is not FnStatement);
        
    public List<Statement> Statements { get; } 

    public ProgramNode(List<Statement> statements)
    {
        Statements = statements;
        Reparent(Statements);
    }
}
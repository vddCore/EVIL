namespace EVIL.Grammar.AST.Statements.TopLevel;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Miscellaneous;

public sealed class FnStatement : Statement
{
    public IdentifierNode Identifier { get; }
    public AttributeList? AttributeList { get; }
    public ParameterList? ParameterList { get; }
    public Statement Statement { get; }
    public bool IsLocalDefinition { get; }

    public FnStatement(
        IdentifierNode identifier,
        AttributeList? attributeList,
        ParameterList? parameterList,
        Statement statement,
        bool isLocalDefinition)
    {
        Identifier = identifier;
        AttributeList = attributeList;
        ParameterList = parameterList;
        Statement = statement;
        IsLocalDefinition = isLocalDefinition;

        Reparent(Identifier);
        Reparent(AttributeList);
        Reparent(ParameterList);
        Reparent(Statement);
    }
}
namespace EVIL.Core.Grammar.AST.Statements;

using System.Collections.Generic;
using System.Linq;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Miscellaneous;

public sealed class ValStatement : Statement
{
    public IReadOnlyList<ValDeclarator> Declarators { get; }
    public Dictionary<IdentifierNode, Expression?> Definitions { get; }
    public bool ReadWrite { get; }
    
    public IEnumerable<IdentifierNode> DeclaredIdentifiers
    {
        get
        {
            foreach (var declarator in Declarators)
            {
                switch (declarator)
                {
                    case IdentifierValDeclarator identifierDeclarator:
                    {
                        yield return identifierDeclarator.Identifier;
                        break;
                    }
                    
                    case ArrayPatternValDeclarator arrayPattern:
                    {
                        foreach (var binding in arrayPattern.Bindings)
                        {
                            yield return binding.Identifier;
                        }

                        break;
                    }
                    
                    case TablePatternValDeclarator tablePattern:
                    {
                        foreach (var field in tablePattern.Fields)
                        {
                            yield return field.Binding.Identifier;
                        }

                        break;
                    }
                }
            }
        }
    }

    public IEnumerable<Expression> Initializers
        => Declarators.Select(declarator => declarator.Initializer)
                      .OfType<Expression>();

    public ValStatement(IReadOnlyList<ValDeclarator> declarators, bool readWrite)
    {
        Declarators = [..declarators];
        Definitions = new Dictionary<IdentifierNode, Expression?>();
        ReadWrite = readWrite;

        foreach (var declarator in Declarators)
        {
            switch (declarator)
            {
                case IdentifierValDeclarator identifierDeclarator:
                {
                    Definitions.Add(
                        identifierDeclarator.Identifier,
                        declarator.Initializer
                    );
                    
                    Reparent(identifierDeclarator);
                    break;
                }

                case ArrayPatternValDeclarator arrayPattern:
                {
                    Reparent(arrayPattern.Bindings);
                    break;
                }

                case TablePatternValDeclarator tablePattern:
                {
                    Reparent(tablePattern.Fields);
                    break;
                }
            }

            Reparent(declarator);
        }
    }
}




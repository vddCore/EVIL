namespace EVIL.Core.Grammar;

using System;
using System.Collections.Generic;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Miscellaneous;
using EVIL.Core.Grammar.AST.Statements;
using EVIL.Core.Lexical;

public partial class Parser
{
    private ValStatement ValStatement(bool readWrite)
    {
        var (line, col) = Match(Token.Val);

        var declarators = new List<ValDeclarator>();

        while (true)
        {
            switch (CurrentToken.Type)
            {
                case TokenType.Identifier:
                    declarators.Add(IdentifierValDeclarator());
                    break;
                case TokenType.LBracket:
                    declarators.Add(ArrayPatternValDeclarator());
                    break;
                case TokenType.LBrace:
                    declarators.Add(TablePatternValDeclarator());
                    break;
                default:
                    throw new ParserException(
                        "Expected an identifier, array pattern, or table pattern in a `val' declaration.",
                        (CurrentToken.Line, CurrentToken.Column)
                    );
            }

            if (CurrentToken.Type == TokenType.Comma)
            {
                Match(Token.Comma);
                continue;
            }

            break;
        }

        return new ValStatement(declarators, readWrite)
            { Line = line, Column = col };
    }

    private IdentifierValDeclarator IdentifierValDeclarator()
    {
        var identifier = Identifier();

        Expression? initializer = null;
        if (CurrentToken.Type == TokenType.Assign)
        {
            Match(Token.Assign);
            initializer = AssignmentExpression();
        }
        else if (CurrentToken.Type == TokenType.LeftArrow)
        {
            throw new ParserException(
                "The '<-' initializer is only supported for array and table patterns.",
                (CurrentToken.Line, CurrentToken.Column)
            );
        }

        return new IdentifierValDeclarator(identifier, initializer);
    }

    private ArrayPatternValDeclarator ArrayPatternValDeclarator()
    {
        var (line, col) = Match(Token.LBracket);
        var bindings = new List<PatternBindingNode>();
        var names = new HashSet<string>(StringComparer.Ordinal);

        if (CurrentToken.Type == TokenType.RBracket)
        {
            throw new ParserException(
                "Empty deconstruction patterns are not supported.",
                (line, col)
            );
        }

        while (true)
        {
            var binding = PatternBinding(names);
            bindings.Add(binding);

            if (CurrentToken.Type != TokenType.Comma)
            {
                break;
            }

            Match(Token.Comma);
            if (CurrentToken.Type == TokenType.RBracket)
            {
                break;
            }
        }

        Match(Token.RBracket);
        var initializer = PatternInitializer();

        return new ArrayPatternValDeclarator(bindings, initializer);
    }

    private TablePatternValDeclarator TablePatternValDeclarator()
    {
        var (line, col) = Match(Token.LBrace);
        var fields = new List<TablePatternField>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        var keys = new HashSet<string>(StringComparer.Ordinal);

        if (CurrentToken.Type == TokenType.RBrace)
        {
            throw new ParserException(
                "Empty deconstruction patterns are not supported.",
                (line, col)
            );
        }

        while (true)
        {
            var readWrite = false;
            if (CurrentToken.Type == TokenType.Rw)
            {
                Match(Token.Rw);
                readWrite = true;
            }

            string key;
            PatternBindingNode bindingNode;
            if (CurrentToken.Type == TokenType.Identifier)
            {
                var keyIdentifier = Identifier();
                key = keyIdentifier.Name;

                if (CurrentToken.Type == TokenType.Colon)
                {
                    Match(Token.Colon);
                    bindingNode = PatternBinding(names, readWrite);
                }
                else
                {
                    bindingNode = PatternBinding(keyIdentifier, readWrite, names);
                }
            }
            else if (CurrentToken.Type == TokenType.LBracket)
            {
                key = TablePatternKey();
                if (CurrentToken.Type != TokenType.Colon)
                {
                    throw new ParserException(
                        "Bracketed table keys must be followed by ':' and a binding.",
                        (CurrentToken.Line, CurrentToken.Column)
                    );
                }

                Match(Token.Colon);
                bindingNode = PatternBinding(names, readWrite);
            }
            else
            {
                throw new ParserException(
                    "Expected an identifier or bracketed plain-string table key.",
                    (CurrentToken.Line, CurrentToken.Column)
                );
            }

            if (!keys.Add(key))
            {
                throw new ParserException(
                    $"Duplicate table key '{key}' in deconstruction pattern.",
                    (bindingNode.Identifier.Line, bindingNode.Identifier.Column)
                );
            }

            fields.Add(new TablePatternField(bindingNode, key));

            if (CurrentToken.Type != TokenType.Comma)
            {
                break;
            }

            Match(Token.Comma);
            if (CurrentToken.Type == TokenType.RBrace)
            {
                break;
            }
        }

        Match(Token.RBrace);
        var initializer = PatternInitializer();

        return new TablePatternValDeclarator(fields, initializer);
    }

    private PatternBindingNode PatternBinding(HashSet<string> names, bool readWrite = false)
    {
        if (CurrentToken.Type == TokenType.Rw)
        {
            Match(Token.Rw);
            readWrite = true;
        }

        if (CurrentToken.Type is TokenType.LBracket or TokenType.LBrace)
        {
            throw new ParserException(
                "Nested deconstruction patterns are not supported.",
                (CurrentToken.Line, CurrentToken.Column)
            );
        }

        if (CurrentToken.Type == TokenType.Ellipsis)
        {
            throw new ParserException(
                "Rest captures are not supported in deconstruction patterns.",
                (CurrentToken.Line, CurrentToken.Column)
            );
        }

        var identifier = Identifier("Expected an identifier in a deconstruction pattern.");
        return PatternBinding(identifier, readWrite, names);
    }

    private PatternBindingNode PatternBinding(
        IdentifierNode identifier,
        bool readWrite,
        HashSet<string> names)
    {
        if (!names.Add(identifier.Name))
        {
            throw new ParserException(
                $"Duplicate binding '{identifier.Name}' in deconstruction pattern.",
                (identifier.Line, identifier.Column)
            );
        }

        return new PatternBindingNode(identifier, readWrite);
    }

    private string TablePatternKey()
    {
        if (CurrentToken.Type == TokenType.Identifier)
        {
            var key = CurrentToken.Value;
            Match(Token.Identifier);
            return key;
        }

        if (CurrentToken.Type == TokenType.LBracket)
        {
            Match(Token.LBracket);
            if (CurrentToken.Type != TokenType.PlainString)
            {
                throw new ParserException(
                    "Bracketed table-pattern keys must be plain string literals; computed keys are not supported.",
                    (CurrentToken.Line, CurrentToken.Column)
                );
            }

            var key = CurrentToken.Value;
            Match(Token.PlainString);
            Match(Token.RBracket);
            return key;
        }

        throw new ParserException(
            "Expected an identifier or bracketed plain-string table key.",
            (CurrentToken.Line, CurrentToken.Column)
        );
    }

    private Expression PatternInitializer()
    {
        if (CurrentToken.Type != TokenType.LeftArrow)
        {
            throw new ParserException(
                "Array and table patterns require a '<-' initializer.",
                (CurrentToken.Line, CurrentToken.Column)
            );
        }

        Match(Token.LeftArrow);
        if (CurrentToken.Type is TokenType.Semicolon or TokenType.Comma or TokenType.EOF)
        {
            throw new ParserException(
                "Expected an initializer after '<-'.",
                (CurrentToken.Line, CurrentToken.Column)
            );
        }

        return AssignmentExpression();
    }
}
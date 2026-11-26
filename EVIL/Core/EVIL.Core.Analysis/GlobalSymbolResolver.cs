namespace EVIL.Core.Analysis;

using System;
using System.Collections.Generic;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Constants;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Miscellaneous;
using EVIL.Core.Grammar.AST.Statements;

public sealed partial class GlobalSymbolResolver
{
    public static readonly GlobalSymbolResolver Default = new();

    private static readonly Dictionary<Type, Func<string, AstNode, bool>> LexicalScopeScanners = new();

    static GlobalSymbolResolver()
    {
        RegisterScanners();
    }

    public static readonly HashSet<string> BuiltInSymbols = new(StringComparer.Ordinal)
    {
        "math", "io", "tbl", "arr", "str", "evil", "assert",
        "dofile", "require", "print", "fs", "time", "core", "debug", 
        "convert"
    };
    
    private readonly HashSet<string> _declaredTopLevelFunctions = new(StringComparer.Ordinal);
    private readonly HashSet<string> _declaredTopLevelVariables = new(StringComparer.Ordinal);
    private readonly HashSet<string> _ambientConfiguredGlobals = new(StringComparer.Ordinal);

    public void IndexProgram(ProgramNode program)
    {
        _declaredTopLevelFunctions.Clear();
        _declaredTopLevelVariables.Clear();

        foreach (var stmt in program.Statements)
        {
            if (stmt is FnStatement fn)
            {
                if (!fn.IsLocalDefinition)
                {
                    _declaredTopLevelFunctions.Add(fn.Identifier.Name);
                }
                else
                {
                    _declaredTopLevelVariables.Add(fn.Identifier.Name);
                }
            }
            else if (stmt is ValStatement val)
            {
                foreach (var identifier in val.DeclaredIdentifiers)
                {
                    _declaredTopLevelVariables.Add(identifier.Name);
                }
            }
        }
    }

    public static bool IsKnownNonNilSingleton(string identifier)
        => BuiltInSymbols.Contains(identifier);

    public bool IsDeclaredTopLevelFunction(string identifier)
        => _declaredTopLevelFunctions.Contains(identifier);

    public bool IsDeclaredTopLevelVariable(string identifier)
        => _declaredTopLevelVariables.Contains(identifier);

    public bool IsAmbientConfigured(string identifier)
        => _ambientConfiguredGlobals.Contains(identifier);

    public void AddAmbientConfigured(string identifier)
        => _ambientConfiguredGlobals.Add(identifier);

    private static void RegisterScanner<TNode>(Func<string, TNode, bool> scanner)
        where TNode : AstNode
    {
        LexicalScopeScanners.Add(typeof(TNode), (identifier, node) => scanner(identifier, (TNode)node));
    }

    public static bool IsInLexicalScope(string identifier, AstNode? fromNode)
    {
        var current = fromNode;

        while (current != null)
        {
            if (LexicalScopeScanners.TryGetValue(current.GetType(), out var scanner)
                && scanner(identifier, current))
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    public bool IsDynamicAmbientGlobal(string identifier, AstNode? fromNode = null)
    {
        if (IsKnownNonNilSingleton(identifier))
        {
            return false;
        }

        if (fromNode != null && IsInLexicalScope(identifier, fromNode))
        {
            return false;
        }

        if (fromNode == null && IsDeclaredTopLevelVariable(identifier))
        {
            return false;
        }

        if (IsDeclaredTopLevelFunction(identifier))
        {
            return false;
        }

        if (IsAmbientConfigured(identifier))
        {
            return false;
        }

        return true;
    }

    public bool IsGuaranteedSafe(Expression expr)
    {
        return expr switch
        {
            TableExpression => true,
            ArrayExpression => true,
            ConstantExpression and not NilConstant => true,
            SelfExpression => true,
            TypeOfExpression => true,
            IsExpression => true,
            NonNilAssertionExpression => true,
            SymbolReferenceExpression sym => IsKnownNonNilSingleton(sym.Identifier) 
                                          || IsDeclaredTopLevelFunction(sym.Identifier),
            _ => false
        };
    }
}

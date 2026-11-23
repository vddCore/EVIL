namespace EVIL.Analysis;

using System;
using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;

public sealed class GlobalSymbolResolver
{
    public static readonly GlobalSymbolResolver Default = new();

    public static readonly HashSet<string> BuiltInSymbols = new(StringComparer.Ordinal)
    {
        "math", "io", "tbl", "arr", "str", "evil", "assert",
        "dofile", "require", "print", "fs", "time", "core", "debug", "convert"
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
                foreach (var def in val.Definitions)
                {
                    _declaredTopLevelVariables.Add(def.Key.Name);
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

    public static bool IsInLexicalScope(string identifier, AstNode? fromNode)
    {
        var current = fromNode;
        while (current != null)
        {
            if (current is BlockStatement block)
            {
                foreach (var stmt in block.Statements)
                {
                    if (stmt is ValStatement val)
                    {
                        foreach (var def in val.Definitions)
                        {
                            if (def.Key.Name == identifier)
                            {
                                return true;
                            }
                        }
                    }
                    else if (stmt is FnStatement { IsLocalDefinition: true } fn 
                             && fn.Identifier?.Name == identifier)
                    {
                        return true;
                    }
                }
            }
            else if (current is FnStatement fn)
            {
                if (fn.ParameterList != null)
                {
                    foreach (var param in fn.ParameterList.Parameters)
                    {
                        if (param.Identifier.Name == identifier)
                        {
                            return true;
                        }
                    }
                }

                if (fn.IsLocalDefinition && fn.Identifier?.Name == identifier)
                {
                    return true;
                }
            }
            else if (current is FnExpression fnExpr)
            {
                if (fnExpr.ParameterList != null)
                {
                    foreach (var param in fnExpr.ParameterList.Parameters)
                    {
                        if (param.Identifier.Name == identifier)
                        {
                            return true;
                        }
                    }
                }
            }
            else if (current is SelfFnExpression selfFn)
            {
                if (selfFn.ParameterList != null)
                {
                    foreach (var param in selfFn.ParameterList.Parameters)
                    {
                        if (param.Identifier.Name == identifier)
                        {
                            return true;
                        }
                    }
                }
            }
            else if (current is FnTargetedStatement fnTargeted)
            {
                if (fnTargeted.ParameterList != null)
                {
                    foreach (var param in fnTargeted.ParameterList.Parameters)
                    {
                        if (param.Identifier.Name == identifier)
                        {
                            return true;
                        }
                    }
                }
            }
            else if (current is FnIndexedStatement fnIndexed)
            {
                if (fnIndexed.ParameterList != null)
                {
                    foreach (var param in fnIndexed.ParameterList.Parameters)
                    {
                        if (param.Identifier.Name == identifier)
                        {
                            return true;
                        }
                    }
                }
            }
            else if (current is EachStatement each)
            {
                if (each.KeyIdentifier?.Name == identifier || each.ValueIdentifier?.Name == identifier)
                {
                    return true;
                }
            }
            else if (current is ForStatement forStmt)
            {
                foreach (var init in forStmt.Assignments)
                {
                    if (init is ValStatement val)
                    {
                        foreach (var def in val.Definitions)
                        {
                            if (def.Key.Name == identifier)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else if (current is TryStatement tryStmt)
            {
                if (tryStmt.HandlerExceptionLocal?.Name == identifier)
                {
                    return true;
                }
            }
            else if (current is ProgramNode prog)
            {
                foreach (var stmt in prog.Statements)
                {
                    if (stmt is ValStatement val)
                    {
                        foreach (var def in val.Definitions)
                        {
                            if (def.Key.Name == identifier)
                            {
                                return true;
                            }
                        }
                    }
                    else if (stmt is FnStatement { IsLocalDefinition: true } progFn 
                             && progFn.Identifier?.Name == identifier)
                    {
                        return true;
                    }
                }
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

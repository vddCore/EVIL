namespace EVIL.Core.Analysis;

using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Miscellaneous;
using EVIL.Core.Grammar.AST.Statements;

public sealed partial class GlobalSymbolResolver
{
    private static void RegisterScanners()
    {
        RegisterScanner<BlockStatement>(BlockStatementScanner);
        RegisterScanner<FnStatement>(FnStatementScanner);
        RegisterScanner<FnExpression>(FnExpressionScanner);
        RegisterScanner<SelfFnExpression>(SelfFnExpressionScanner);
        RegisterScanner<FnTargetedStatement>(FnTargetedStatementScanner);
        RegisterScanner<FnIndexedStatement>(FnIndexedStatementScanner);
        RegisterScanner<EachStatement>(EachStatementScanner);
        RegisterScanner<ForStatement>(ForStatementScanner);
        RegisterScanner<TryStatement>(TryStatementScanner);
        RegisterScanner<ProgramNode>(ProgramNodeScanner);
    }

    private static bool BlockStatementScanner(string identifier, BlockStatement block)
    {
        foreach (var stmt in block.Statements)
        {
            if (stmt is ValStatement val)
            {
                foreach (var identifierNode in val.DeclaredIdentifiers)
                {
                    if (identifierNode.Name == identifier)
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

        return false;
    }

    private static bool FnStatementScanner(string identifier, FnStatement fn)
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

        if (fn.IsLocalDefinition && fn.Identifier.Name == identifier)
        {
            return true;
        }

        return false;
    }
    
    private static bool FnExpressionScanner(string identifier, FnExpression fnExpr)
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

        return false;
    }

    private static bool SelfFnExpressionScanner(string identifier, SelfFnExpression selfFn)
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

        return false;
    }

    private static bool FnTargetedStatementScanner(string identifier, FnTargetedStatement fnTargeted)
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

        return false;
    }

    private static bool FnIndexedStatementScanner(string identifier, FnIndexedStatement fnIndexed)
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

        return false;
    }

    private static bool EachStatementScanner(string identifier, EachStatement each)
    {
        if (each.KeyIdentifier?.Name == identifier 
            || each.ValueIdentifier?.Name == identifier)
        {
            return true;
        }

        return false;
    }

    private static bool ForStatementScanner(string identifier, ForStatement forStmt)
    {
        foreach (var init in forStmt.Assignments)
        {
            if (init is ValStatement val)
            {
                foreach (var identifierNode in val.DeclaredIdentifiers)
                {
                    if (identifierNode.Name == identifier)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    
    private static bool TryStatementScanner(string identifier, TryStatement tryStmt)
    {
        if (tryStmt.HandlerExceptionLocal?.Name == identifier)
        {
            return true;
        }

        return false;
    }

    private static bool ProgramNodeScanner(string identifier, ProgramNode program)
    {
        foreach (var stmt in program.Statements)
        {
            if (stmt is ValStatement val)
            {
                foreach (var identifierNode in val.DeclaredIdentifiers)
                {
                    if (identifierNode.Name == identifier)
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

        return false;
    }
}
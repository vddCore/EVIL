namespace EVIL.Ceres.Runtime.Extensions;

using System.Linq;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

public static class TableExtensions
{
    public static DynamicValue SetUsingPath<TSubTable>(this Table table, string fullyQualifiedName, DynamicValue value, bool replaceIfExists = true)
        where TSubTable: Table, new()
    {
        var segments = fullyQualifiedName.Split(".");
        var tablePath = segments
            .SkipLast(1)
            .ToArray();

        var memberName = segments.Last();
        var currentTable = table;
        foreach (var tableName in tablePath)
        {
            if (!currentTable.Contains(tableName))
            {
                currentTable.Set(
                    tableName,
                    new TSubTable()
                );
            }
            else
            {
                if (currentTable[tableName].Type != DynamicValueType.Table)
                {
                    throw new EvilRuntimeException(
                        $"Attempted to set '{value}' as '{fullyQualifiedName}', but '{tableName}' is not a Table."
                    );
                }
            }

            currentTable = currentTable[tableName].Table!;
        }

        if (currentTable.Contains(memberName) && !replaceIfExists)
        {
            throw new EvilRuntimeException(
                $"Attempt to set '{value}' as '{fullyQualifiedName}', but '{memberName}' already exists in the target Table."
            );
        }

        if (currentTable.IsFrozen)
        {
            throw new EvilRuntimeException(
                $"Attempt to set '{value}' as '{fullyQualifiedName}', but the target Table is frozen."
            );
        }

        currentTable.Set(memberName, value);
        return value;
    }

    public static bool ContainsPath(this Table table, string fullyQualifiedName)
    {
        var segments = fullyQualifiedName.Split(".");
        var tablePath = segments
            .SkipLast(1)
            .ToArray();

        var memberName = segments.Last();
        var currentTable = table;
        foreach (var tableName in tablePath)
        {
            if (!(currentTable.Contains(tableName) && currentTable[tableName].Type == DynamicValueType.Table))
            {
                return false;
            }

            currentTable = currentTable[tableName].Table!;
        }

        return currentTable.Contains(memberName);
    }
        
    public static bool ContainsValuePath(this PropertyTable table, string fullyQualifiedName)
    {
        var segments = fullyQualifiedName.Split(".");
        var tablePath = segments
            .SkipLast(1)
            .ToArray();

        var memberName = segments.Last();
        var currentTable = (Table)table;
        foreach (var tableName in tablePath)
        {
            if (!(currentTable.Contains(tableName) && currentTable[tableName].Type == DynamicValueType.Table))
            {
                return false;
            }

            currentTable = currentTable[tableName].Table!;
        }

        if (currentTable is not PropertyTable pt)
            return false;
            
        return pt.ContainsValue(memberName);
    }
        
    public static bool ContainsGetterPath(this PropertyTable table, string fullyQualifiedName)
    {
        var segments = fullyQualifiedName.Split(".");
        var tablePath = segments
            .SkipLast(1)
            .ToArray();

        var memberName = segments.Last();
        var currentTable = (Table)table;
        foreach (var tableName in tablePath)
        {
            if (!(currentTable.Contains(tableName) && currentTable[tableName].Type == DynamicValueType.Table))
            {
                return false;
            }

            currentTable = currentTable[tableName].Table!;
        }

        if (currentTable is not PropertyTable pt)
            return false;
            
        return pt.ContainsGetter(memberName);
    }
        
    public static bool ContainsSetterPath(this PropertyTable table, string fullyQualifiedName)
    {
        var segments = fullyQualifiedName.Split(".");
        var tablePath = segments
            .SkipLast(1)
            .ToArray();

        var memberName = segments.Last();
        var currentTable = (Table)table;
        foreach (var tableName in tablePath)
        {
            if (!(currentTable.Contains(tableName) && currentTable[tableName].Type == DynamicValueType.Table))
            {
                return false;
            }

            currentTable = currentTable[tableName].Table!;
        }

        if (currentTable is not PropertyTable pt)
            return false;
            
        return pt.ContainsSetter(memberName);
    }
}
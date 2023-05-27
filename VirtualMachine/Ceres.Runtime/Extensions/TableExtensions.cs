using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.Runtime.Extensions
{
    public static class TableExtensions
    {
        public static DynamicValue SetUsingPath(this Table table, string fullyQualifiedName, DynamicValue value, bool replaceIfExists = true)
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
                        new Table()
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
                if (!(currentTable!.Contains(tableName) && currentTable[tableName].Type == DynamicValueType.Table))
                {
                    return false;
                }

                currentTable = currentTable[tableName].Table!;
            }

            return currentTable.Contains(memberName);
        }
    }
}
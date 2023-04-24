using Ceres.ExecutionEngine;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.Runtime
{
    public sealed class EvilRuntime
    {
        private readonly CeresVM _vm;

        private Table Global => _vm.Global;

        public EvilRuntime(CeresVM vm)
        {
            _vm = vm;
        }

        public DynamicValue Register(string fullyQualifiedName, DynamicValue value, bool replaceIfExists = false)
        {
            var segments = fullyQualifiedName.Split(".");
            var tablePath = segments
                .SkipLast(1)
                .ToArray();

            var funcName = segments.Last();
            var currentTable = Global;
            foreach (var tableName in tablePath)
            {
                if (!currentTable!.Contains(tableName))
                {
                    currentTable.Set(
                        tableName,
                        new DynamicValue(new Table())
                    );
                }
                else
                {
                    if (currentTable[tableName].Type != DynamicValueType.Table)
                    {
                        throw new EvilRuntimeException(
                            $"Attempted to register '{value}' as '{fullyQualifiedName}', but '{tableName}' is not a Table."
                        );
                    }
                }

                currentTable = currentTable[tableName].Table!;
            }

            if (currentTable.Contains(funcName) && !replaceIfExists)
            {
                throw new EvilRuntimeException(
                    $"Attempt to register '{value}' as '{fullyQualifiedName}', but '{funcName}' already exists in the target Table."
                );
            }

            if (currentTable.IsFrozen)
            {
                throw new EvilRuntimeException(
                    $"Attempt to register '{value}' as '{fullyQualifiedName}', but the target Table is frozen."
                );
            }

            currentTable.Set(funcName, value);
            return value;
        }

        public DynamicValue Register(string fullyQualifiedName, double value, bool replaceIfExists = false)
            => Register(fullyQualifiedName, new DynamicValue(value), replaceIfExists);

        public DynamicValue Register(string fullyQualifiedName, string value, bool replaceIfExists = false)
            => Register(fullyQualifiedName, new DynamicValue(value), replaceIfExists);

        public DynamicValue Register(string fullyQualifiedName, bool value, bool replaceIfExists = false)
            => Register(fullyQualifiedName, new DynamicValue(value), replaceIfExists);

        public DynamicValue Register(string fullyQualifiedName, Table value, bool replaceIfExists = false)
            => Register(fullyQualifiedName, new DynamicValue(value), replaceIfExists);

        public DynamicValue Register(string fullyQualifiedName, Chunk value, bool replaceIfExists = false)
            => Register(fullyQualifiedName, new DynamicValue(value), replaceIfExists);

        public DynamicValue Register(string fullyQualifiedName, Fiber value, bool replaceIfExists = false)
            => Register(fullyQualifiedName, new DynamicValue(value), replaceIfExists);

        public DynamicValue Register(string fullyQualifiedName, NativeFunction value, bool replaceIfExists = false)
            => Register(fullyQualifiedName, new DynamicValue(value), replaceIfExists);
    }
}
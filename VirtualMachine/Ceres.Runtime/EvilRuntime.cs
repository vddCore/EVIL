using System.Reflection;
using Ceres.ExecutionEngine;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Modules;
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

        public void RegisterBuiltInModules()
        {
            RegisterModule<CoreModule>();
            RegisterModule<TimeModule>();
        }

        public void RegisterModule<T>() where T : EvilRuntimeModule
        {
            try
            {
                var members = FindRegistrableRuntimeMembers(typeof(T));

                foreach (var kvp in members)
                {
                    Register(kvp.Key, kvp.Value, true);
                }
            }
            catch (Exception e)
            {
                throw new EvilRuntimeException(
                    $"There was an issue registering a runtime function member, or the entire module. " +
                    $"See inner exception for details.",
                    e
                );
            }
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

        private Dictionary<string, NativeFunction> FindRegistrableRuntimeMembers(Type type)
        {
            var validRuntimeMembers = new Dictionary<string, NativeFunction>();

            if (!type.IsAssignableTo(typeof(EvilRuntimeModule)))
            {
                throw new InvalidOperationException(
                    $"A type must inherit from {nameof(EvilRuntimeModule)} to be considered a valid runtime module."
                );
            }

            if (!type.IsSealed)
            {
                throw new InvalidOperationException(
                    "A type must be sealed to be considered a valid runtime module."
                );
            }

            if (type.GetConstructors(BindingFlags.Public).Any())
            {
                throw new InvalidOperationException(
                    "A type must not have any public constructors to be considered a valid runtime module."
                );
            }

            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                var methodAttr = method.GetCustomAttribute<EvilRuntimeMemberAttribute>();

                if (methodAttr == null)
                {
                    continue;
                }

                NativeFunction nativeFunction;
                try
                {
                    nativeFunction = method.CreateDelegate<NativeFunction>();
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(
                        $"Method '{method.Name}' from '{type.FullName}' does not have a valid signature.",
                        e
                    );
                }

                if (Global.IndexUsingFullyQualifiedName(methodAttr.FullyQualifiedName) != DynamicValue.Nil)
                {
                    if (!methodAttr.AllowRedefinition)
                    {
                        throw new InvalidOperationException(
                            $"'{methodAttr.FullyQualifiedName}' is already defined, and the runtime member attribute" +
                            $"for method '{method.Name}' in '{type.FullName}' doesn't allow redefinition."
                        );
                    }
                }

                validRuntimeMembers.Add(methodAttr.FullyQualifiedName, nativeFunction);
            }

            return validRuntimeMembers;
        }
    }
}
using System;
using System.Reflection;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Interop;
using EVIL.Runtime.Library;

namespace EVIL.Runtime
{
    public class EvilRuntime
    {
        public Table TargetTable { get; }

        public EvilRuntime(Table targetTable)
        {
            TargetTable = targetTable;
        }
        
        public void LoadCoreRuntime()
        {
            RegisterPackage<CoreModule>();
            RegisterPackage<FileSystemModule>();
            RegisterPackage<InputOutputModule>();
            RegisterPackage<MathModule>();
            RegisterPackage<StringModule>();
            RegisterPackage<TableModule>();
            RegisterPackage<TimeModule>();
        }

        public void RegisterPackage<T>()
            => RegisterPackage(typeof(T));

        public void RegisterPackage(Type type)
        {
            var libAttr = type.GetCustomAttribute<ClrLibraryAttribute>();

            if (libAttr != null)
            {
                if (TargetTable.IsSet(libAttr.LibraryName))
                {
                    var entry = TargetTable.Get(libAttr.LibraryName);
                    if (entry.Type != DynamicValueType.Table)
                    {
                        throw new InvalidOperationException(
                            $"Cannot register the package '{type.Name}' - a global variable '{libAttr.LibraryName}' exists and it is not a table."
                        );
                    }
                }
                else
                {
                    TargetTable.Set(
                        libAttr.LibraryName,
                        new DynamicValue(new Table())
                    );
                }
            }

            foreach (var m in type.GetMethods())
            {
                if (!m.IsPublic
                    || m.Name == "GetType"
                    || m.Name == "ToString"
                    || m.Name == "Equals"
                    || m.Name == "GetHashCode")
                    continue;

                if (!m.IsStatic)
                {
                    throw new InvalidOperationException(
                        $"Cannot register method '{m.Name}' from package '{type.Name} ({libAttr?.LibraryName})' - it's not static."
                    );
                }

                var funcAttr = m.GetCustomAttribute<ClrFunctionAttribute>();

                if (funcAttr == null)
                    continue;

                var clrDelegate = m.CreateDelegate<ClrFunction>();

                if (libAttr != null)
                {
                    RegisterFunction(
                        libAttr.LibraryName,
                        funcAttr.Name,
                        clrDelegate
                    );
                }
                else
                {
                    RegisterFunction(
                        funcAttr.Name, 
                        clrDelegate
                    );
                }
            }
        }
        
        private void RegisterFunction(string name, ClrFunction function)
            => TargetTable.Set(name, new(function));
        
        public void RegisterFunction(string libraryName, string functionName, ClrFunction function)
        {
            if (!TargetTable.IsSet(libraryName))
            {
                throw new InvalidOperationException(
                    "Internal failure. Tried to register a function inside a non-existent library-defined table."
                );
            }

            var libTable = TargetTable.Get(libraryName).Table;
            libTable.Set(functionName, new DynamicValue(function));
        }
    }
}
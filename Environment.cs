using System;
using System.Collections.Generic;
using System.Reflection;
using EVIL.Abstraction;
using EVIL.Runtime;
using EVIL.Runtime.Library;
using static EVIL.Execution.Interpreter;

namespace EVIL
{
    public class Environment
    {
        public Dictionary<string, DynValue> Globals { get; }
        public Dictionary<string, ClrFunction> BuiltIns { get; }
        public Dictionary<string, ScriptFunction> Functions { get; }

        public Environment()
        {
            Globals = new Dictionary<string, DynValue>();

            BuiltIns = new Dictionary<string, ClrFunction>();
            Functions = new Dictionary<string, ScriptFunction>();
        }

        public void LoadCoreRuntime()
        {
            RegisterPackage<CoreLibrary>();
            RegisterPackage<StringLibrary>();
            RegisterPackage<TableLibrary>();
            RegisterPackage<MathLibrary>();
            RegisterPackage<TimeLibrary>();
        }

        public void RegisterPackage<T>()
        {
            var type = typeof(T);

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
                        $"Cannot register method '{m.Name}' from package '{type.Name}' - it's not static."
                    );
                }

                var attr = m.GetCustomAttribute(typeof(ClrFunctionAttribute)) as ClrFunctionAttribute;

                if (attr == null)
                    continue;

                RegisterBuiltIn(
                    attr.Name,
                    m.CreateDelegate<ClrFunction>()
                );
            }
        }

        public void RegisterBuiltIn(string name, ClrFunction clrFunction)
        {
            if (BuiltIns.ContainsKey(name))
                throw new InvalidOperationException($"Built-in function '{name}' has already been registered.");
            BuiltIns.Add(name, clrFunction);
        }

        public void RegisterFunction(string name, ScriptFunction function)
        {
            if (Functions.ContainsKey(name))
            {
                Functions[name] = function;
                return;
            }

            Functions.Add(name, function);
        }
    }
}
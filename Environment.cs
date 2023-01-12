using System;
using System.Collections.Generic;
using EVIL.Abstraction;
using EVIL.Execution;
using EVIL.RuntimeLibrary;
using EVIL.RuntimeLibrary.Base;
using static EVIL.Execution.Interpreter;

namespace EVIL
{
    public class Environment
    {
        private readonly Interpreter _interpreter;

        public Dictionary<string, DynValue> Globals { get; }
        public Dictionary<string, ClrFunction> BuiltIns { get; }
        public Dictionary<string, ScriptFunction> Functions { get; }

        public Dictionary<string, DynValue> SupplementLocalLookupTable { get; }

        public Environment(Interpreter interpreter)
        {
            _interpreter = interpreter;

            Globals = new Dictionary<string, DynValue>();

            BuiltIns = new Dictionary<string, ClrFunction>();
            Functions = new Dictionary<string, ScriptFunction>();

            SupplementLocalLookupTable = new Dictionary<string, DynValue>();
        }

        public void LoadCoreRuntime()
        {
            RegisterPackage<CoreLibrary>();
            RegisterPackage<StringLibrary>();
            RegisterPackage<TableLibrary>();
            RegisterPackage<MathLibrary>();
            RegisterPackage<TimeLibrary>();
        }

        public void RegisterPackage<T>() where T: ClrPackage
        {
            var packageInstance = Activator.CreateInstance<T>();
            packageInstance.Register(this, _interpreter);
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

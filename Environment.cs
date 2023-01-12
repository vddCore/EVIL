using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EVIL.Abstraction;
using EVIL.Abstraction.Base;
using EVIL.Diagnostics;
using EVIL.Execution;
using EVIL.Runtime;
using EVIL.Runtime.Library;

namespace EVIL
{
    public class Environment
    {
        public int CallStackLimit { get; set; } = 256;

        public Stack<CallStackItem> CallStack { get; }
        public Stack<LoopStackItem> LoopStack { get; }
        public Stack<NameScope> NameScopes { get; }

        public bool IsInScriptFunctionScope => CallStack.Count > 0;
        public bool IsInsideLoop => LoopStack.Count > 0;

        public CallStackItem CallStackTop => CallStack.Peek();
        public LoopStackItem LoopStackTop => LoopStack.Peek();

        public NameScope LocalScope => NameScopes.Peek();
        public NameScope GlobalScope => NameScopes.ElementAt(NameScopes.Count - 1);

        public Environment()
        {
            CallStack = new Stack<CallStackItem>();
            LoopStack = new Stack<LoopStackItem>();

            NameScopes = new Stack<NameScope>();
            NameScopes.Push(new NameScope(null));
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

                var d = m.CreateDelegate<Func<Interpreter, ClrFunctionArguments, DynValue>>();

                RegisterFunction(
                    attr.Name,
                    new ClrFunction(d)
                );
            }
        }

        public void RegisterFunction(string name, IFunction function)
        {
            if (function is ClrFunction clrFunction)
            {
                GlobalScope.Set(name, new DynValue(clrFunction));
            }
            else if (function is ScriptFunction scriptFunction)
            {
                if (IsInScriptFunctionScope)
                {
                    LocalScope.Set(name, new DynValue(scriptFunction));
                }
                else
                {
                    GlobalScope.Set(name, new DynValue(scriptFunction));
                }
            }
        }
        
        public void Clear()
        {
            LoopStack.Clear();
            CallStack.Clear();
        }

        public List<CallStackItem> StackTrace()
        {
            return new(CallStack);
        }

        public NameScope EnterScope()
        {
            NameScopes.Push(new NameScope(LocalScope));
            return NameScopes.Peek();
        }

        public void ExitScope()
        {
            NameScopes.Pop();
        }
    }
}
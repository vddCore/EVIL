using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EVIL.Abstraction;
using EVIL.Diagnostics;
using EVIL.Execution;
using EVIL.Runtime;
using EVIL.Runtime.Library;

namespace EVIL
{
    public class Environment
    {
        public int CallStackLimit { get; set; } = 64;

        public Stack<StackFrame> CallStack { get; }
        public Stack<LoopFrame> LoopStack { get; }

        public bool IsInScriptFunctionScope => CallStack.Count > 0;
        public bool IsInsideLoop => LoopStack.Count > 0;

        public StackFrame StackTop => CallStack.Peek();
        public LoopFrame LoopStackTop => LoopStack.Peek();

        public NameScope LocalScope { get; private set; }
        public NameScope GlobalScope { get; }
        private Stack<NameScope> EnclosedScopes { get; } = new();

        public Environment()
        {
            CallStack = new Stack<StackFrame>();
            LoopStack = new Stack<LoopFrame>();

            GlobalScope = new NameScope(this, null);
            LocalScope = GlobalScope;
        }

        public void LoadCoreRuntime()
        {
            RegisterPackage<CoreLibrary>();
            RegisterPackage<IoLibrary>();
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

        public void RegisterFunction(string name, ClrFunction clrFunction)
        {
            GlobalScope.Set(name, new DynValue(clrFunction));
        }

        public void RegisterFunction(string name, ScriptFunction scriptFunction)
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

        public void Clear()
        {
            LoopStack.Clear();
            CallStack.Clear();
            EnclosedScopes.Clear();

            LocalScope = GlobalScope;
        }

        public List<StackFrame> StackTrace()
        {
            return new(CallStack);
        }

        public NameScope EnterScope(bool enclose = false)
        {
            if (enclose)
            {
                EnclosedScopes.Push(LocalScope);
                LocalScope = new NameScope(this, null);
            }
            else
            {
                LocalScope = new NameScope(this, LocalScope);
            }

            return LocalScope;
        }

        public void ExitScope()
        {
            LocalScope = LocalScope.ParentScope;

            if (LocalScope == null)
            {
                if (EnclosedScopes.Any())
                {
                    LocalScope = EnclosedScopes.Pop();
                }
                else
                {
                    LocalScope = GlobalScope;
                }
            }
        }
    }
}
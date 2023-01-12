using System;
using System.Collections.Generic;
using System.Reflection;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;
using EVIL.Interpreter.Runtime;
using EVIL.Interpreter.Runtime.Library;

namespace EVIL.Interpreter
{
    public class Environment
    {
        public int CallStackLimit { get; set; } = 72;

        public Stack<StackFrame> CallStack { get; }
        public Stack<LoopFrame> LoopStack { get; }

        public bool IsInScriptFunctionScope => CallStack.Count > 0;
        public bool IsInsideLoop => LoopStack.Count > 0;

        public StackFrame StackTop => CallStack.Peek();
        public LoopFrame LoopStackTop => LoopStack.Peek();

        public NameScope LocalScope { get; private set; }
        public NameScope GlobalScope { get; }

        public Environment()
        {
            CallStack = new Stack<StackFrame>();
            LoopStack = new Stack<LoopFrame>();

            GlobalScope = new NameScope(this, null);
            LocalScope = GlobalScope;
        }

        public void LoadCoreRuntime()
        {
            RegisterPackage<FileSystemLibrary>();
            RegisterPackage<CoreLibrary>();
            RegisterPackage<IoLibrary>();
            RegisterPackage<StringLibrary>();
            RegisterPackage<TableLibrary>();
            RegisterPackage<MathLibrary>();
            RegisterPackage<TimeLibrary>();
        }

        public void RegisterPackage<T>()
            => RegisterPackage(typeof(T));

        public void RegisterPackage(Type type)
        {
            var libAttr = type.GetCustomAttribute<ClrLibraryAttribute>();

            if (libAttr != null)
            {
                if (GlobalScope.HasMember(libAttr.LibraryName))
                {
                    if (GlobalScope.Members[libAttr.LibraryName].Type != DynValueType.Table)
                    {
                        throw new InvalidOperationException(
                            $"Cannot register the package '{type.Name}' - a global variable '{libAttr.LibraryName}' exists and it is not a table."
                        );
                    }
                }
                else
                {
                    GlobalScope.Set(libAttr.LibraryName, new DynValue(new Table()));
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

                var d = m.CreateDelegate<Func<Execution.Interpreter, FunctionArguments, DynValue>>();

                if (libAttr != null)
                {
                    RegisterFunction(
                        libAttr.LibraryName,
                        funcAttr.Name,
                        new ClrFunction(d)
                    );
                }
                else
                {
                    RegisterFunction(
                        funcAttr.Name,
                        new ClrFunction(d)
                    );
                }
            }
        }

        public void RegisterFunction(string libraryName, string functionName, ClrFunction function)
        {
            if (!GlobalScope.HasMember(libraryName))
            {
                throw new InvalidOperationException(
                    "Internal failure. Tried to register a function inside a non-existent library-defined table."
                );
            }

            GlobalScope.Members[libraryName].Table[functionName] = new DynValue(function);
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

            LocalScope = GlobalScope;
        }

        public List<StackFrame> StackTrace()
        {
            return new(CallStack);
        }

        public NameScope EnterScope()
        {
            LocalScope = new NameScope(this, LocalScope);
            return LocalScope;
        }

        public void ExitScope()
        {
            LocalScope = LocalScope.ParentScope;

            if (LocalScope == null)
            {
                LocalScope = GlobalScope;
            }
        }
    }
}
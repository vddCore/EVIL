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

        public Stack<StackFrame> CallStack { get; } = new();
        public StackFrame StackTop => CallStack.Peek();

        public NameScope LocalScope { get; set; }
        public NameScope GlobalScope { get; private set; }

        public Environment()
        {
            Clear();
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
            var scope = LocalScope ?? GlobalScope;
            scope.Set(name, new DynValue(scriptFunction));
        }

        public void Clear()
        {
            CallStack.Clear();            
            LocalScope = null;
            GlobalScope = new NameScope(this, null);
        }

        public void Begin()
        {
            CallStack.Push(new StackFrame("!root_chunk!"));
        }

        public void End()
        {
            CallStack.Pop();
        }

        public List<StackFrame> StackTrace()
            => new(CallStack);

        public NameScope EnterScope()
            => (LocalScope = new NameScope(this, LocalScope));

        public void ExitScope()
            => LocalScope = LocalScope?.ParentScope;
    }
}
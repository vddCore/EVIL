using System.Reflection;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;

namespace Ceres.Runtime
{
    public abstract class RuntimeModule : Table
    {
        public delegate DynamicValue TableGetter(DynamicValue key);
        public delegate (DynamicValue Key, DynamicValue Value) TableSetter(DynamicValue key, DynamicValue value);

        private readonly Dictionary<DynamicValue, TableGetter> _getters = new();
        private readonly Dictionary<DynamicValue, TableSetter> _setters = new();

        public abstract string FullyQualifiedName { get; }

        public RuntimeModule()
        {
            RegisterNativeFunctions();
            Freeze(true);
        }

        public DynamicValue AttachTo(Table table)
        {
            var ret = this;

            table.SetUsingPath(
                FullyQualifiedName,
                ret
            );

            return ret;
        }

        protected virtual void AddGetter(DynamicValue key, TableGetter getter)
        {
            if (!_getters.TryAdd(key, getter))
            {
                throw new InvalidOperationException(
                    $"Attempt to register a duplicate getter for '{key.ToString()}'"
                );
            }
        }

        protected virtual bool RemoveGetter(DynamicValue key)
            => _getters.Remove(key);

        protected virtual void AddSetter(DynamicValue key, TableSetter getter)
        {
            if (!_setters.TryAdd(key, getter))
            {
                throw new InvalidOperationException(
                    $"Attempt to register a duplicate setter for '{key.ToString()}'"
                );
            }
        }

        protected virtual bool RemoveSetter(DynamicValue key)
            => _setters.Remove(key);

        protected override DynamicValue OnIndex(DynamicValue key)
        {
            if (_getters.TryGetValue(key, out var getter))
            {
                return getter(key);
            }

            return base.OnIndex(key);
        }

        protected override (DynamicValue Key, DynamicValue Value) OnSet(DynamicValue key, DynamicValue value)
        {
            if (_setters.TryGetValue(key, out var setter))
            {
                return setter(key, value);
            }

            return base.OnSet(key, value);
        }

        protected override bool OnContains(DynamicValue key)
        {
            if (_getters.TryGetValue(key, out _))
            {
                return true;
            }

            if (_setters.TryGetValue(key, out _))
            {
                return true;
            }

            return base.OnContains(key);
        }

        private void RegisterNativeFunctions()
        {
            var validFunctions = GetType().GetMethods(
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
            ).Where(HasSupportedFunctionSignature)
             .Where(HasRequiredFunctionAttribute)
             .Select(x => 
             {
                var nativeFunction = x.CreateDelegate<NativeFunction>(null);
                var attribute = x.GetCustomAttribute<RuntimeModuleFunctionAttribute>()!;

                return (Function: nativeFunction, Attribute: attribute);
            });
            
            foreach (var tuple in validFunctions)
            {
                if (this.ContainsPath(tuple.Attribute.SubNameSpace))
                {
                    throw new InvalidOperationException(
                        $"Attempt to register a duplicate member with name '{tuple.Attribute.SubNameSpace}' " +
                        $"in '{FullyQualifiedName}'."
                    );
                }
                
                this.SetUsingPath(tuple.Attribute.SubNameSpace, new(tuple.Function));
            }
        }

        private bool HasRequiredFunctionAttribute(MethodInfo method)
            => method.GetCustomAttribute<RuntimeModuleFunctionAttribute>() != null;

        private bool HasSupportedFunctionSignature(MethodInfo method)
        {
            var parameters = method.GetParameters();

            return parameters.Length == 2
                   && parameters[0].ParameterType.IsAssignableTo(typeof(Fiber))
                   && parameters[1].ParameterType.IsAssignableTo(typeof(DynamicValue[]));
        }
    }
}
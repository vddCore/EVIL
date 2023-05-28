using System.Reflection;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;

namespace Ceres.Runtime
{
    public abstract class RuntimeModule : PropertyTable
    {
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
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
            RegisterNativeGetters();
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
                   && parameters[1].ParameterType.IsAssignableTo(typeof(DynamicValue[]))
                   && method.ReturnType.IsAssignableTo(typeof(DynamicValue));
        }

        private void RegisterNativeGetters()
        {
            var validGetters = GetType().GetMethods(
                    BindingFlags.Public
                    | BindingFlags.NonPublic
                    | BindingFlags.Static
                ).Where(HasSupportedGetterSignature)
                .Where(HasRequiredGetterAttribute)
                .Select(x =>
                {
                    var getter = x.CreateDelegate<TableGetter>(null);
                    var attribute = x.GetCustomAttribute<RuntimeModuleGetterAttribute>()!;

                    return (Getter: getter, Attribute: attribute);
                });

            foreach (var tuple in validGetters)
            {
                if (this.ContainsPath(tuple.Attribute.SubNameSpace))
                {
                    throw new InvalidOperationException(
                        $"Attempt to register a duplicate member with name '{tuple.Attribute.SubNameSpace}' " +
                        $"in '{FullyQualifiedName}'."
                    );
                }

                if (string.IsNullOrEmpty(tuple.Attribute.SubNameSpace))
                    continue;

                var subNameSpaceParts = tuple.Attribute.SubNameSpace.Split('.');
                var targetName = subNameSpaceParts[subNameSpaceParts.Length - 1];

                if (subNameSpaceParts.Length >= 2)
                {
                    var subNameSpace = string.Join('.', subNameSpaceParts[0..^1]);

                    var dynval = IndexUsingFullyQualifiedName(subNameSpace);

                    PropertyTable propTable;
                    
                    if (dynval == DynamicValue.Nil)
                    {
                        propTable = new PropertyTable();
                    }
                    else
                    {
                        propTable = (PropertyTable)dynval.Table!;
                    }
                    
                    propTable.AddGetter(targetName, tuple.Getter);
                    this.SetUsingPath(subNameSpace, propTable);
                }
                else
                {
                    AddGetter(targetName, tuple.Getter);
                }
            }
        }

        private bool HasRequiredGetterAttribute(MethodInfo method)
            => method.GetCustomAttribute<RuntimeModuleGetterAttribute>() != null;

        private bool HasSupportedGetterSignature(MethodInfo method)
        {
            var parameters = method.GetParameters();

            return parameters.Length == 1
                   && parameters[0].ParameterType.IsAssignableTo(typeof(DynamicValue))
                   && method.ReturnType.IsAssignableTo(typeof(DynamicValue));
        }
    }
}
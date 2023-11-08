using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;

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

            table.SetUsingPath<Table>(
                FullyQualifiedName,
                ret
            );

            return ret;
        }

        public string Describe()
        {
            var functions = GetType().GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            var sb = new StringBuilder();

            foreach (var function in functions)
            {
                var docString = DescribeFunction(function);
                if (docString != null)
                {
                    sb.AppendLine(docString);
                }
            }

            return sb.ToString();
        }

        public string? DescribeFunction(MethodInfo methodInfo, bool blockQuote = true)
        {
            var sb = new StringBuilder();

            var runtimeModuleFunctionAttribute = methodInfo.GetCustomAttribute<RuntimeModuleFunctionAttribute>();
            var evilDocFunctionAttribute = methodInfo.GetCustomAttribute<EvilDocFunctionAttribute>();
            var evilDocArgumentAttributes = methodInfo.GetCustomAttributes<EvilDocArgumentAttribute>().ToArray();
            
            if (runtimeModuleFunctionAttribute == null)
                return null;

            if (evilDocFunctionAttribute == null)
                return null;
            
            sb.Append($"#### `{FullyQualifiedName}.{runtimeModuleFunctionAttribute.SubNameSpace}");
            sb.Append("(");
            {
                for (var i = 0; i < evilDocArgumentAttributes.Length; i++)
                {
                    var argInfo = evilDocArgumentAttributes[i];

                    if (argInfo.DefaultValue != null)
                    {
                        var defaultValue = argInfo.DefaultValue;
                        if (argInfo.Type == DynamicValueType.String)
                        {
                            defaultValue = $"\"{defaultValue}\"";
                        }
                        
                        sb.Append($"[{argInfo.Name}] = {defaultValue}");
                    }
                    else
                    {
                        sb.Append(argInfo.Name);
                    }
                    
                    if (i < evilDocArgumentAttributes.Length - 1 || evilDocFunctionAttribute.IsVariadic)
                    {
                        sb.Append(", ");
                    }
                }
            }

            if (evilDocFunctionAttribute.IsVariadic)
            {
                sb.Append("...");
            }
            sb.Append($")");

            if (evilDocFunctionAttribute.IsAnyReturn)
            {
                sb.AppendLine($" -> Any`");
            }
            else
            {
                sb.AppendLine($" -> {GetTypeString(evilDocFunctionAttribute.ReturnType)}`  ");
            }

            sb.AppendLine("**Description**  ");
            sb.AppendLine(evilDocFunctionAttribute.Description);
            sb.AppendLine();
            sb.AppendLine("**Returns**  ");
            sb.AppendLine(evilDocFunctionAttribute.Returns);

            if (evilDocArgumentAttributes.Any())
            {
                sb.AppendLine();
                sb.AppendLine("**Arguments**  ");
                sb.AppendLine("| Name | Description | Type | Optional | Default Value |  ");
                sb.AppendLine("| ---- | ----------- | ---- | -------- | ------------- |  ");
                for (var i = 0; i < evilDocArgumentAttributes.Length; i++)
                {
                    var argInfo = evilDocArgumentAttributes[i];
                    var argType = argInfo.IsAnyType ? "Any" : GetTypeString(argInfo.Type);
                    
                    var defaultValue = argInfo.DefaultValue == null 
                        ? "None" 
                        : $"`{argInfo.DefaultValue}`";

                    var optional = argInfo.DefaultValue == null ? "No" : "Yes";
                    
                    if (argInfo.Type == DynamicValueType.String)
                    {
                        defaultValue = $"\"{defaultValue}\"";
                    }

                    sb.AppendLine($"| `{argInfo.Name}` | {argInfo.Description} | `{argType}` | {optional} | {defaultValue} |  ");
                }
            }

            if (!blockQuote)
            {
                return sb.ToString();
            }
            else
            {
                var lines = sb.ToString().Split('\n').Select(x => x.Trim('\r')).ToList();

                for (var i = 0; i < lines.Count; i++)
                {
                    if (lines[i] != "---")
                    {
                        lines[i] = $"> {lines[i]}";
                    }
                }
                lines.Add("");
                return string.Join('\n', lines);
            }
        }
        
        private static string GetTypeString(DynamicValueType type)
            => new DynamicValue(type).ConvertToString().String!;

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

                this.SetUsingPath<PropertyTable>(
                    tuple.Attribute.SubNameSpace,
                    new(tuple.Function)
                );
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
                    this.SetUsingPath<PropertyTable>(subNameSpace, propTable);
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
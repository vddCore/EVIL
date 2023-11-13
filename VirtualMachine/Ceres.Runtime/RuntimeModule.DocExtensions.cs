using System.Linq;
using System.Reflection;
using System.Text;
using Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.Runtime
{
    public static class RuntimeModuleDocExtensions
    {
        public static string? DescribeProperty(this RuntimeModule module, MethodInfo methodInfo, bool blockQuote = true)
        {
            var sb = new StringBuilder();

            var runtimeModuleGetterAttribute = methodInfo.GetCustomAttribute<RuntimeModuleGetterAttribute>();
            var evilDocPropertyAttribute = methodInfo.GetCustomAttribute<EvilDocPropertyAttribute>();
            
            if (runtimeModuleGetterAttribute == null)
                return null;


            sb.AppendLine($"## {module.FullyQualifiedName}.{runtimeModuleGetterAttribute.SubNameSpace}");

            if (evilDocPropertyAttribute != null)
            {
                sb.AppendLine("**Synopsis**  ");
                sb.AppendLine($"**`{module.FullyQualifiedName}.{runtimeModuleGetterAttribute.SubNameSpace}`**  ");

                if (evilDocPropertyAttribute.Mode.HasFlag(EvilDocPropertyMode.Get))
                {
                    sb.Append($"**` :: get");
                    if (evilDocPropertyAttribute.IsAnyGet)
                    {
                        sb.AppendLine($" -> Any`**\n  ");
                    }
                    else
                    {
                        sb.AppendLine($" -> {GetTypeString(evilDocPropertyAttribute.ReturnType)}`**\n  ");
                    }
                }

                if (evilDocPropertyAttribute.Mode.HasFlag(EvilDocPropertyMode.Set))
                {
                    sb.Append($"**` ::set");
                    if (evilDocPropertyAttribute.IsAnySet)
                    {
                        sb.AppendLine($" <- Any`**\n  ");
                    }
                    else
                    {
                        if (evilDocPropertyAttribute.InputTypes.Any())
                        {
                            if (evilDocPropertyAttribute.InputTypes.Length > 1)
                            {
                                sb.AppendLine(
                                    $" <- ({string.Join(',', evilDocPropertyAttribute.InputTypes.Select(GetTypeString))})`**\n  ");
                            }
                            else
                            {
                                sb.AppendLine($" <- {GetTypeString(evilDocPropertyAttribute.InputTypes[0])}`**\n  ");
                            }
                        }
                    }
                }

                sb.AppendLine("**Description**  ");
                sb.AppendLine(evilDocPropertyAttribute.Description);
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine("This property has no available documentation.");
            }

            if (!blockQuote)
            {
                return sb.ToString();
            }
            else
            {
                var lines = sb.ToString()
                    .Split('\n')
                    .Select(x => x.Trim('\r'))
                    .ToList();

                for (var i = 0; i < lines.Count; i++)
                {
                    lines[i] = $"> {lines[i]}";
                }
                
                lines.Add("");
                return string.Join('\n', lines);
            }
        }
        
        public static string? DescribeFunction(this RuntimeModule module, MethodInfo methodInfo, bool blockQuote = true)
        {
            var sb = new StringBuilder();

            var runtimeModuleFunctionAttribute = methodInfo.GetCustomAttribute<RuntimeModuleFunctionAttribute>();
            var evilDocFunctionAttribute = methodInfo.GetCustomAttribute<EvilDocFunctionAttribute>();
            var evilDocArgumentAttributes = methodInfo.GetCustomAttributes<EvilDocArgumentAttribute>().ToArray();
            
            if (runtimeModuleFunctionAttribute == null)
                return null;

            sb.AppendLine($"## {module.FullyQualifiedName}.{runtimeModuleFunctionAttribute.SubNameSpace}");

            if (evilDocFunctionAttribute != null)
            {
                sb.AppendLine("**Synopsis**  ");
                sb.Append($"`{module.FullyQualifiedName}.{runtimeModuleFunctionAttribute.SubNameSpace}");
                sb.Append("(");
                {
                    for (var i = 0; i < evilDocArgumentAttributes.Length; i++)
                    {
                        var argInfo = evilDocArgumentAttributes[i];

                        if (argInfo.Name == "...")
                        {
                            continue;
                        }

                        var argName = argInfo.Name;
                        if (argInfo.CanBeNil)
                        {
                            argName += "?";
                        }

                        if (argInfo.DefaultValue != null)
                        {
                            var defaultValue = argInfo.DefaultValue;
                            if (argInfo.PrimaryType == DynamicValueType.String)
                            {
                                defaultValue = $"\"{defaultValue}\"";
                            }

                            sb.Append($"[{argName}] = {defaultValue}");
                        }
                        else
                        {
                            sb.Append(argName);
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

                sb.Append($")  ");

                if (evilDocFunctionAttribute.IsAnyReturn)
                {
                    sb.AppendLine($" -> Any`\n  ");
                }
                else
                {
                    sb.AppendLine($" -> {GetTypeString(evilDocFunctionAttribute.ReturnType)}`\n  ");
                }

                sb.AppendLine("**Description**  ");
                sb.AppendLine(evilDocFunctionAttribute.Description);

                if (!string.IsNullOrEmpty(evilDocFunctionAttribute.Returns))
                {
                    sb.AppendLine();
                    sb.AppendLine("**Returns**  ");
                    sb.AppendLine(evilDocFunctionAttribute.Returns);
                }

                if (evilDocArgumentAttributes.Any())
                {
                    sb.AppendLine();
                    sb.AppendLine("**Arguments**  ");
                    sb.AppendLine("| Name | Description | Type(s) | Can be `nil` | Optional | Default Value |  ");
                    sb.AppendLine("| ---- | ----------- | ------- | ------------ | -------- | ------------- |  ");
                    for (var i = 0; i < evilDocArgumentAttributes.Length; i++)
                    {
                        var argInfo = evilDocArgumentAttributes[i];
                        var argType = argInfo.IsAnyType ? "Any" : GetTypeString(argInfo.PrimaryType);

                        if (!argInfo.IsAnyType && argInfo.OtherTypes != null)
                        {
                            foreach (var otherType in argInfo.OtherTypes)
                            {
                                argType += $", {GetTypeString(otherType)}";
                            }
                        }

                        var defaultValue = "None";

                        if (argInfo.DefaultValue != null)
                        {
                            if (argInfo.PrimaryType == DynamicValueType.String)
                            {
                                defaultValue = $"`\"{argInfo.DefaultValue}\"`";
                            }
                            else
                            {
                                defaultValue = $"`{argInfo.DefaultValue}`";
                            }
                        }

                        var optional = argInfo.DefaultValue == null ? "No" : "Yes";
                        var canBeNil = argInfo.CanBeNil ? "Yes" : "No";

                        sb.AppendLine($"| `{argInfo.Name}` | {argInfo.Description} | `{argType}` | {canBeNil} | {optional} | {defaultValue} |  ");
                    }
                }
            }
            else
            {
                sb.AppendLine("This function has no available documentation.");
            }

            if (!blockQuote)
            {
                return sb.ToString();
            }
            else
            {
                var lines = sb.ToString()
                    .Split('\n')
                    .Select(x => x.Trim('\r'))
                    .ToList();

                for (var i = 0; i < lines.Count; i++)
                {
                    lines[i] = $"> {lines[i]}";
                }
                
                lines.Add("");
                return string.Join('\n', lines);
            }
        }
        
        public static string DescribeProperties(this RuntimeModule module)
        {
            var functions = module.GetType().GetMethods(
                BindingFlags.Static 
                | BindingFlags.NonPublic 
                | BindingFlags.Public
            );

            var sb = new StringBuilder();

            foreach (var function in functions)
            {
                var docString = module.DescribeProperty(function);
                if (docString != null)
                {
                    sb.AppendLine(docString);
                }
            }

            return sb.ToString();
        }
        
        public static string DescribeFunctions(this RuntimeModule module)
        {
            var functions = module.GetType().GetMethods(
                BindingFlags.Static 
                | BindingFlags.NonPublic 
                | BindingFlags.Public
            );

            var sb = new StringBuilder();

            foreach (var function in functions)
            {
                var docString = module.DescribeFunction(function);
                if (docString != null)
                {
                    sb.AppendLine(docString);
                }
            }

            return sb.ToString();
        }

        public static string Describe(this RuntimeModule module)
        {
            var sb = new StringBuilder();

            var propertyDescriptionString = module.DescribeProperties();
            var functionDescriptionString = module.DescribeFunctions();
            
            sb.AppendLine($"# EVIL Runtime Module `{module.FullyQualifiedName}`");

            if (!string.IsNullOrEmpty(propertyDescriptionString))
            {
                sb.AppendLine($"## Properties");
                sb.AppendLine(propertyDescriptionString);
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(functionDescriptionString))
            {
                sb.AppendLine($"## Functions");
                sb.AppendLine(functionDescriptionString);
                sb.AppendLine();
            }

            return sb.ToString();
        }
        
        private static string GetTypeString(DynamicValueType type)
            => new DynamicValue(type).ConvertToString().String!;

    }
}
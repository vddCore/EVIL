﻿namespace EVIL.Ceres.Runtime;

using System;
using System.Linq;
using System.Reflection;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime.Extensions;

public abstract class RuntimeModule : PropertyTable
{
    public const string GlobalMergeKey = "<global>";
        
    public abstract string FullyQualifiedName { get; }

    public RuntimeModule()
    {
        RegisterNativeFunctions();
        RegisterNativeSetters();
        RegisterNativeGetters();
    }

    public DynamicValue AttachTo(Table table)
    {
        var ret = this;

        if (FullyQualifiedName == GlobalMergeKey)
        {
            foreach (var (k, v) in this) table[k] = v;
        }
        else
        {
            table.SetUsingPath<Table>(
                FullyQualifiedName,
                ret
            );
        }

        return ret;
    }

    internal void Registered(EvilRuntime runtime)
        => OnRegistered(runtime);
        
    protected virtual void OnRegistered(EvilRuntime runtime)
    {
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
                    $"Attempt to register a duplicate member (native function) with name '{tuple.Attribute.SubNameSpace}' " +
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
            if (this.ContainsValuePath(tuple.Attribute.SubNameSpace) || 
                this.ContainsGetterPath(tuple.Attribute.SubNameSpace))
            {
                throw new InvalidOperationException(
                    $"Attempt to register a duplicate member (getter) with name '{tuple.Attribute.SubNameSpace}' " +
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

    private void RegisterNativeSetters()
    {
        var validSetters = GetType().GetMethods(
                BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.Static
            ).Where(HasSupportedSetterSignature)
            .Where(HasRequiredSetterAttribute)
            .Select(x =>
            {
                var setter = x.CreateDelegate<TableSetter>(null);
                var attribute = x.GetCustomAttribute<RuntimeModuleSetterAttribute>()!;

                return (Setter: setter, Attribute: attribute);
            });
            
        foreach (var tuple in validSetters)
        {
            if (this.ContainsValuePath(tuple.Attribute.SubNameSpace) || 
                this.ContainsSetterPath(tuple.Attribute.SubNameSpace))
            {
                throw new InvalidOperationException(
                    $"Attempt to register a duplicate member (setter) with name '{tuple.Attribute.SubNameSpace}' " +
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

                propTable.AddSetter(targetName, tuple.Setter);
                this.SetUsingPath<PropertyTable>(subNameSpace, propTable);
            }
            else
            {
                AddSetter(targetName, tuple.Setter);
            }
        }
    }
        
    private bool HasRequiredSetterAttribute(MethodInfo method)
        => method.GetCustomAttribute<RuntimeModuleSetterAttribute>() != null;

    private bool HasSupportedSetterSignature(MethodInfo method)
    {
        var parameters = method.GetParameters();

        return parameters.Length == 2
               && parameters[0].ParameterType.IsAssignableTo(typeof(DynamicValue))
               && parameters[1].ParameterType.IsAssignableTo(typeof(DynamicValue))
               && method.ReturnType.IsAssignableTo(typeof((DynamicValue, DynamicValue)));
    }
}
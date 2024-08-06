namespace EVIL.Ceres.Runtime;

using System;

[AttributeUsage(AttributeTargets.Method)]
public class RuntimeModuleSetterAttribute : Attribute
{
    public string SubNameSpace { get; }

    public RuntimeModuleSetterAttribute(string subNameSpace)
    {
        SubNameSpace = subNameSpace;
    }
}
namespace EVIL.Ceres.Runtime;

using System;

[AttributeUsage(AttributeTargets.Method)]
public class RuntimeModuleGetterAttribute : Attribute
{
    public string SubNameSpace { get; }

    public RuntimeModuleGetterAttribute(string subNameSpace)
    {
        SubNameSpace = subNameSpace;
    }
}
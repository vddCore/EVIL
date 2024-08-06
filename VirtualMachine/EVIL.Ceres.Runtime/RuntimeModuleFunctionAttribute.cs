namespace EVIL.Ceres.Runtime;

using System;

[AttributeUsage(AttributeTargets.Method)]
public class RuntimeModuleFunctionAttribute : Attribute
{
    public string SubNameSpace { get; }
        
    public RuntimeModuleFunctionAttribute(string subNameSpace)
    {
        SubNameSpace = subNameSpace;
    }
}
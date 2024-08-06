namespace EVIL.Ceres.Runtime;

using System;
using EVIL.CommonTypes.TypeSystem;

[AttributeUsage(AttributeTargets.Method)]
public class EvilDocThrowsAttribute : Attribute
{
    public DynamicValueType ThrownType { get; }
    public string Description { get; }

    public EvilDocThrowsAttribute(DynamicValueType thrownType, string description)
    {
        ThrownType = thrownType;
        Description = description;
    }
}
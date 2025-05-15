namespace EVIL.Ceres.Runtime;

using System;
using EVIL.CommonTypes.TypeSystem;

[AttributeUsage(AttributeTargets.Method)]
public class EvilDocPropertyAttribute(
    EvilDocPropertyMode mode,
    string description
) : Attribute
{
    public EvilDocPropertyMode Mode { get; } = mode;
    public string Description { get; } = description;

    public bool IsAnyGet { get; set; } = false;
    public bool IsAnySet { get; set; } = false;
        
    public DynamicValueType ReturnType { get; set; } = DynamicValueType.Nil;
    public DynamicValueType[] InputTypes { get; set; } = [];
}
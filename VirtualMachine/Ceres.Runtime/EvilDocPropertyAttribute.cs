using System;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EvilDocPropertyAttribute : Attribute
    {
        public EvilDocPropertyMode Mode { get; }
        public string Description { get; }

        public bool IsAnyGet { get; set; } = false;
        public bool IsAnySet { get; set; } = false;
        
        public DynamicValueType ReturnType { get; set; } = DynamicValueType.Nil;
        public DynamicValueType InputType { get; set; } = DynamicValueType.Nil;

        public EvilDocPropertyAttribute(EvilDocPropertyMode mode, string description)
        {
            Mode = mode;
            Description = description;
        } 
    }
}
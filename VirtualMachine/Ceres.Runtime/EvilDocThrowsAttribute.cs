using System;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.Runtime
{
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
}
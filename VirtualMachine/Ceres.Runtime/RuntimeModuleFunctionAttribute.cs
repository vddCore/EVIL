using System;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RuntimeModuleFunctionAttribute : Attribute
    {
        public string SubNameSpace { get; }
        public DynamicValueType ReturnType { get; set; }

        public RuntimeModuleFunctionAttribute(string subNameSpace)
        {
            SubNameSpace = subNameSpace;
        }
    }
}
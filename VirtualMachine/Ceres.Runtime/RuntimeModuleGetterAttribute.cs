using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.Runtime
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RuntimeModuleGetterAttribute : Attribute
    {
        public string SubNameSpace { get; }
        public DynamicValue.DynamicValueType ReturnType { get; set; }

        public RuntimeModuleGetterAttribute(string subNameSpace)
        {
            SubNameSpace = subNameSpace;
        }
    }
}
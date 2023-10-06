using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public class ClosureInfo
    {
        internal DynamicValue Value { get; set; }
        
        public int NestingLevel { get; }
        public int EnclosedId { get; }
        
        public bool IsParameter { get; }
        public bool IsClosure { get; }

        internal ClosureInfo(int nestingLevel, int enclosedId, bool isParameter, bool isClosure)
        {
            NestingLevel = nestingLevel;
            EnclosedId = enclosedId;
            IsParameter = isParameter;
            IsClosure = isClosure;
        }
    }
}
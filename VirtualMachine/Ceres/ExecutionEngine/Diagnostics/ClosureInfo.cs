using System;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public record ClosureInfo
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

        public virtual bool Equals(ClosureInfo? other)
        {
            return NestingLevel == other?.NestingLevel
                && EnclosedId == other?.EnclosedId
                && IsParameter == other?.IsParameter
                && IsClosure == other?.IsClosure;
        }

        public override int GetHashCode()
            => HashCode.Combine(
                NestingLevel,
                EnclosedId,
                IsParameter,
                IsClosure
            );
    }
}
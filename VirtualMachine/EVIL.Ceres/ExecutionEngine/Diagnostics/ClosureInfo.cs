namespace EVIL.Ceres.ExecutionEngine.Diagnostics;

using System;

public record ClosureInfo
{
    public int NestingLevel { get; }
    public int EnclosedId { get; }
    public string EnclosedFunctionName { get; }
        
    public bool IsParameter { get; }
    public bool IsClosure { get; }
        
    public bool IsSharedScope { get; }

    internal ClosureInfo(
        int nestingLevel,
        int enclosedId,
        string enclosedFunctionName,
        bool isParameter,
        bool isClosure,
        bool isSharedScope)
    {
        NestingLevel = nestingLevel;
        EnclosedId = enclosedId;
        EnclosedFunctionName = enclosedFunctionName;
        IsParameter = isParameter;
        IsClosure = isClosure;
        IsSharedScope = isSharedScope;
    }

    public virtual bool Equals(ClosureInfo? other)
    {
        return NestingLevel == other?.NestingLevel
               && EnclosedId == other.EnclosedId
               && EnclosedFunctionName == other.EnclosedFunctionName
               && IsParameter == other.IsParameter
               && IsClosure == other.IsClosure
               && IsSharedScope == other.IsSharedScope;
    }

    public override int GetHashCode()
        => HashCode.Combine(
            NestingLevel,
            EnclosedId,
            EnclosedFunctionName,
            IsParameter,
            IsClosure,
            IsSharedScope
        );
}
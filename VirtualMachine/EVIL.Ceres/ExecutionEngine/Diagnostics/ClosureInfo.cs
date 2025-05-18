namespace EVIL.Ceres.ExecutionEngine.Diagnostics;

using System;

public record ClosureInfo
{
    public int NestingLevel { get; }
    public int EnclosedId { get; }
    public string EnclosedFunctionName { get; }
        
    public ClosureType Type { get; }
        
    public bool IsSharedScope { get; }

    internal ClosureInfo(
        int nestingLevel,
        int enclosedId,
        string enclosedFunctionName,
        ClosureType type,
        bool isSharedScope)
    {
        NestingLevel = nestingLevel;
        EnclosedId = enclosedId;
        EnclosedFunctionName = enclosedFunctionName;
        Type = type;
        IsSharedScope = isSharedScope;
    }

    public virtual bool Equals(ClosureInfo? other)
    {
        return NestingLevel == other?.NestingLevel
               && EnclosedId == other.EnclosedId
               && EnclosedFunctionName == other.EnclosedFunctionName
               && Type == other.Type
               && IsSharedScope == other.IsSharedScope;
    }

    public override int GetHashCode()
        => HashCode.Combine(
            NestingLevel,
            EnclosedId,
            EnclosedFunctionName,
            Type,
            IsSharedScope
        );
}
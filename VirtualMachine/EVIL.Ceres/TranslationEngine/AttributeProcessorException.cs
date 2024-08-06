namespace EVIL.Ceres.TranslationEngine;

using System;
using EVIL.Ceres.ExecutionEngine.Diagnostics;

public class AttributeProcessorException : Exception
{
    public ChunkAttribute Attribute { get; }
    public Chunk Chunk { get; }

    internal AttributeProcessorException(
        ChunkAttribute attribute,
        Chunk chunk,
        Exception innerException) 
        : base($"An error occured processing attribute '{attribute}' in chunk '{chunk}'.", innerException)
    {
        Attribute = attribute;
        Chunk = chunk;
    }
}
using System;

namespace Ceres.ExecutionEngine.Diagnostics
{
    [Flags]
    public enum ChunkFlags
    {
        Empty = 0,
        
        HasName = 1 << 0,
        HasParameters = 1 << 1,
        HasParameterInitializers = 1 << 2,
        HasLocals = 1 << 3,
        HasLabels = 1 << 4,
        HasAttributes = 1 << 5,
        HasDebugInfo = 1 << 6,
        HasClosures = 1 << 7,
        HasSubChunks = 1 << 8
    }
}
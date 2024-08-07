﻿namespace EVIL.Ceres.ExecutionEngine.Diagnostics;

using System;

[Flags]
public enum ChunkFlags
{
    Empty = 0,
    HasParameters = 1 << 0,
    HasParameterInitializers = 1 << 1,
    HasLocals = 1 << 2,
    HasLabels = 1 << 3,
    HasAttributes = 1 << 4,
    HasDebugInfo = 1 << 5,
    HasClosures = 1 << 6,
    HasSubChunks = 1 << 7,
    IsSubChunk = 1 << 8,
    IsSelfAware = 1 << 9,
    IsSpecialName = 1 << 10,
    HasProtectedBlocks = 1 << 11,
    MayThrow = 1 << 12
}
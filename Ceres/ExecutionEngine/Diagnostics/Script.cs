using System;
using System.Collections.Generic;
using System.Linq;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed class Script
    {
        public int MainChunkID { get; set; }

        public List<Chunk> Chunks { get; } = new();

        public Chunk FindChunkByName(string name)
        {
            return Chunks.FirstOrDefault(x => x.Name == name)
                   ?? throw new InvalidOperationException($"Chunk {name} not found.");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Marshaling;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed class Script : IDynamicValueProvider
    {
        public int MainChunkID { get; set; }

        public List<Chunk> Chunks { get; } = new();

        public Chunk? this[string name]
        {
            get
            {
                try
                {
                    return FindChunkByName(name);
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
        }

        public Chunk? this[int id]
        {
            get
            {
                if (id < 0 || id >= Chunks.Count)
                    return null;

                return Chunks[id];
            }
        }

        public Chunk FindChunkByName(string name)
        {
            return Chunks.FirstOrDefault(x => x.Name == name)
                   ?? throw new InvalidOperationException($"Chunk {name} not found.");
        }

        public bool TryFindChunkByName(string name, out Chunk chunk)
        {
            chunk = null!;

            try
            {
                chunk = FindChunkByName(name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public DynamicValue ToDynamicValue()
        {
            var chunks = new Table();

            for (var i = 0; i < Chunks.Count; i++)
            {
                var chunk = Chunks[i];
                chunks[chunk.Name!] = chunk;
            }

            return new Table
            {
                { "main_chunk", MainChunkID },
                { "chunks", chunks }
            };
        }
    }
}
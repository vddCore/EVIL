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
        private List<Chunk> _chunks = new();

        public IReadOnlyList<Chunk> Chunks => _chunks;

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

        public Chunk CreateChunk(string name, out bool replacedExisting, out Chunk replacedChunk)
        {
            replacedExisting = false;
            replacedChunk = null!;
            
            var chunk = new Chunk(name);
            AddChunk(chunk, out replacedExisting, out replacedChunk);
            return chunk;
        }

        public void AddChunk(Chunk chunk, out bool replacedExisting, out Chunk replacedChunk)
        {
            replacedExisting = false;
            replacedChunk = null!;

            if (TryFindChunkByName(chunk.Name, out replacedChunk))
            {
                _chunks.Remove(replacedChunk);
                replacedExisting = true;
            }

            _chunks.Add(chunk);
        }

        public bool RemoveChunk(string name)
        {
            if (!TryFindChunkByName(name, out var chunk))
                return false;

            return _chunks.Remove(chunk);
        }

        public bool RemoveChunk(Chunk chunk)
            => _chunks.Remove(chunk);

        public Chunk FindChunkByName(string name)
        {
            return Chunks.FirstOrDefault(x => x.Name == name)
                   ?? throw new InvalidOperationException($"Chunk {name} not found.");
        }

        public bool ChunkExists(string name)
            => Chunks.FirstOrDefault(x => x.Name == name) != null;

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
                { "chunks", chunks }
            };
        }
    }
}
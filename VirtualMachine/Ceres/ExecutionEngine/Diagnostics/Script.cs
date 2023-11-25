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
        private Dictionary<string, Chunk> _chunkLookup = new();
        private List<Chunk> _chunks = new();

        public IReadOnlyList<Chunk> Chunks => _chunks;

        public Chunk? this[string name]
        {
            get
            {
                if (_chunkLookup.TryGetValue(name, out var chunk))
                {
                    return chunk;
                }
                
                return null;
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
                _chunkLookup.Remove(chunk.Name);
                
                replacedExisting = true;
            }

            _chunks.Add(chunk);
            _chunkLookup.Add(chunk.Name, chunk);
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
            Chunk? chunk = null;

            _chunkLookup.TryGetValue(name, out chunk);
            
            return chunk ?? throw new InvalidOperationException($"Chunk {name} not found.");
        }

        public bool ChunkExists(string name)
            => Chunks.FirstOrDefault(x => x.Name == name) != null;

        public bool TryFindChunkByName(string name, out Chunk chunk)
        {
            chunk = null!;
            return _chunkLookup.TryGetValue(name, out chunk!);
        }

        public DynamicValue ToDynamicValue()
        {
            var chunks = new Table();

            for (var i = 0; i < Chunks.Count; i++)
            {
                var chunk = Chunks[i];
                chunks[chunk.Name] = chunk;
            }

            return new Table
            {
                { "chunks", chunks }
            };
        }
    }
}
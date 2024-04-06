using System.Collections.Generic;
using Ceres.ExecutionEngine.Diagnostics;

namespace Ceres.LanguageTests
{
    public class TestSet
    {
        private readonly Dictionary<string, Chunk> _testRootChunks = new();
        
        public string DirectoryPath { get; }
        public IReadOnlyDictionary<string, Chunk> TestRootChunks => _testRootChunks;

        public TestSet(string directoryPath)
        {
            DirectoryPath = directoryPath;
        }

        public bool AddTestRootChunk(string filePath, Chunk chunk)
            => _testRootChunks.TryAdd(filePath, chunk);
    }
}
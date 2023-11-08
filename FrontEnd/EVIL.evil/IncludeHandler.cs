using System;
using System.Collections.Generic;
using System.IO;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine;

namespace EVIL.evil
{
    public class IncludeHandler
    {
        private readonly Dictionary<string, IEnumerable<Chunk>> _includeCache = new();
        private readonly List<string> _includeSearchPaths = new();

        public IReadOnlyList<string> IncludeSearchPaths => _includeSearchPaths;
        public IReadOnlyDictionary<string, IEnumerable<Chunk>> IncludeCache => _includeCache;

        public IncludeHandler(Compiler compiler)
        {
            ReadIncludeDefaultsIfAny();

            compiler.RegisterIncludeProcessor(ProcessIncludeFile);
        }

        public void AddIncludeSearchPath(string path)
        {
            var absolutePath = path;
            if (!Path.IsPathFullyQualified(absolutePath))
            {
                absolutePath = Path.GetFullPath(absolutePath);
            }

            if (!Directory.Exists(absolutePath))
            {
                throw new DirectoryNotFoundException($"Could not find the specified include directory '{path}' - it does not exist.");
            }

            if (!_includeSearchPaths.Exists(x => x == absolutePath))
            {
                _includeSearchPaths.Add(absolutePath);
            }
        }

        private void ReadIncludeDefaultsIfAny()
        {
            var thisAssemblyPath = AppContext.BaseDirectory;
            var includesPath = Path.Combine(thisAssemblyPath, "config/default.includes");

            if (!File.Exists(includesPath))
            {
                return;
            }

            var lines = File.ReadAllLines(includesPath);
            foreach (var line in lines)
            {
                var path = line
                    .Replace("$EVILHOME", thisAssemblyPath)
                    .Replace("$APPDATA", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

                if (Directory.Exists(path))
                {
                    AddIncludeSearchPath(path);
                }
            }
        }

        private IEnumerable<Chunk> ProcessIncludeFile(
            Compiler requestingCompiler,
            Script scriptBeingBuilt,
            string includedFilePath,
            out bool isRedundantInclude)
        {
            isRedundantInclude = false;
            
            var localPath = Path.Combine(
                Path.GetDirectoryName(requestingCompiler.CurrentFileName)!,
                includedFilePath
            );

            if (localPath == requestingCompiler.CurrentFileName)
            {
                throw new InvalidOperationException(
                    $"Recursive inclusion detected in '{requestingCompiler.CurrentFileName}'."
                );
            }

            if (_includeCache.TryGetValue(localPath, out var chunks))
            {
                isRedundantInclude = true;
                return chunks;
            }

            if (File.Exists(localPath))
            {
                _includeCache.Add(localPath, Process(localPath));
                return _includeCache[localPath];
            }

            foreach (var path in _includeSearchPaths)
            {
                var fullPathToInclude = Path.Combine(path, includedFilePath);

                if (File.Exists(fullPathToInclude))
                {
                    _includeCache.Add(fullPathToInclude, Process(fullPathToInclude));
                    return _includeCache[fullPathToInclude];
                }
            }
            
            throw new FileNotFoundException(
                $"Cannot find the included file '{includedFilePath}' in any known include search paths."
            );

        }

        private IEnumerable<Chunk> Process(string fullPathToInclude)
        {
            var includeCompiler = new Compiler();
            includeCompiler.RegisterIncludeProcessor(ProcessIncludeFile);

            try
            {
                return includeCompiler.Compile(
                    File.ReadAllText(fullPathToInclude),
                    fullPathToInclude
                ).Chunks;
            }
            catch (CompilerException ce)
            {
                throw new Exception(
                    $"Failed to compile the included file '{fullPathToInclude}':\n{ce.Log}", ce
                );
            }
        }
    }
}
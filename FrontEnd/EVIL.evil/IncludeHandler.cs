using System;
using System.Collections.Generic;
using System.IO;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine;

namespace EVIL.evil
{
    public class IncludeHandler
    {
        private readonly List<string> _includeSearchPaths = new();

        public IReadOnlyList<string> IncludeSearchPaths => _includeSearchPaths;

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
            string includedFilePath)
        {
            var localPath = Path.Combine(
                Path.GetDirectoryName(requestingCompiler.CurrentFileName)!,
                includedFilePath
            );

            if (File.Exists(localPath))
            {
                return Process(localPath);
            }

            foreach (var path in _includeSearchPaths)
            {
                var fullPathToInclude = Path.Combine(path, includedFilePath);

                if (File.Exists(fullPathToInclude))
                {
                    return Process(fullPathToInclude);
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
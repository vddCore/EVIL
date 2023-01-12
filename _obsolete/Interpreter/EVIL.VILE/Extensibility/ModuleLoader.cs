using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EVIL.Interpreter.Runtime;

namespace EVIL.VILE.Extensibility
{
    public class ModuleLoader
    {
        private string _directory;

        private List<string> _modulePaths = new();
        private List<Type> _libraryTypes = new();

        public IReadOnlyList<Type> Libraries => _libraryTypes;

        public ModuleLoader(string directory)
        {
            _directory = directory;

            SearchForModules();
            LoadModules();
        }

        private void SearchForModules()
        {
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
                return;
            }

            _modulePaths.AddRange(
                Directory.GetFiles(
                    _directory,
                    "*.dll",
                    SearchOption.AllDirectories
                )
            );
        }

        private void LoadModules()
        {
            foreach (var path in _modulePaths)
            {
                try
                {
                    var asm = Assembly.LoadFrom(path);

                    foreach (var t in asm.GetExportedTypes())
                    {
                        var attr = t.GetCustomAttribute<ClrLibraryAttribute>();

                        if (attr != null)
                            _libraryTypes.Add(t);
                    }
                }
                catch
                {
                    // Ignore.
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Ceres.Runtime;

namespace EVIL.evil
{
    public class RuntimeModuleLoader
    {
        private const string RuntimeModuleExtensionPattern = "*.evrm.dll";

        private readonly EvilRuntime _runtime;

        public RuntimeModuleLoader(EvilRuntime runtime)
        {
            _runtime = runtime;
        }

        public List<RuntimeModule> RegisterUserRuntimeModules(string directoryPath)
        {
            var filePaths = Directory.GetFiles(directoryPath, $"{RuntimeModuleExtensionPattern}");
            var modules = new List<RuntimeModule>();


            foreach (var filePath in filePaths)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(filePath);
                    var types = assembly.GetExportedTypes().Where(t => t.IsAssignableTo(typeof(RuntimeModule)));

                    foreach (var type in types)
                    {
                        modules.Add(_runtime.RegisterModule(type, out _));
                    }
                }
                catch (Exception e)
                {
                    throw new RuntimeModuleLoadException("Failed to load a user runtime module.", e, filePath);
                }
            }


            return modules;
        }
    }
}
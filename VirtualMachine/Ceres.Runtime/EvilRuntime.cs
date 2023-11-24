using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Ceres.ExecutionEngine;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using Ceres.Runtime.Modules;
using Ceres.TranslationEngine;

namespace Ceres.Runtime
{
    public sealed class EvilRuntime
    {
        private readonly CeresVM _vm;
        private readonly Compiler _compiler;

        private Table Global => _vm.Global;

        public EvilRuntime(CeresVM vm)
        {
            _vm = vm;
            _compiler = new Compiler();
        }

        public List<Chunk> RegisterBuiltInFunctions()
        {
            var scriptNames = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceNames()
                .Where(x => x.StartsWith("Ceres.Runtime.ScriptBuiltins"));

            var list = new List<Chunk>();

            foreach (var scriptName in scriptNames)
            {
                using var stream = Assembly
                    .GetExecutingAssembly()
                    .GetManifestResourceStream(scriptName)!;

                using var streamReader = new StreamReader(stream);

                var text = streamReader.ReadToEnd();
                var root = _compiler.Compile(text, $"builtin::{scriptName}");

                foreach (var (name, id) in root.NamedSubChunkLookup)
                {
                    list.Add(root.SubChunks[id]);
                    _vm.Global.Set(name, root.SubChunks[id]);
                }
            }

            return list;
        }

        public List<RuntimeModule> RegisterBuiltInModules()
        {
            var modules = new List<RuntimeModule>();

            modules.Add(RegisterModule<ArrayModule>(out _));
            modules.Add(RegisterModule<ConvertModule>(out _));
            modules.Add(RegisterModule<CoreModule>(out _));
            modules.Add(RegisterModule<EvilModule>(out _));
            modules.Add(RegisterModule<FsModule>(out _));
            modules.Add(RegisterModule<IoModule>(out _));
            modules.Add(RegisterModule<MathModule>(out _));
            modules.Add(RegisterModule<StringModule>(out _));
            modules.Add(RegisterModule<TableModule>(out _));
            modules.Add(RegisterModule<TimeModule>(out _));

            return modules;
        }

        public T RegisterModule<T>(out DynamicValue table) where T : RuntimeModule, new()
        {
            var module = new T();
            table = module.AttachTo(Global);

            return module;
        }

        public RuntimeModule RegisterModule(Type type, out DynamicValue table)
        {
            var module = (RuntimeModule)Activator.CreateInstance(type)!;
            table = module.AttachTo(Global);

            return module;
        }

        public DynamicValue Register(string fullyQualifiedName, DynamicValue value, bool replaceIfExists = true)
            => Global.SetUsingPath<PropertyTable>(fullyQualifiedName, value, replaceIfExists);
    }
}
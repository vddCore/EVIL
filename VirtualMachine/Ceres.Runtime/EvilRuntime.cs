using System.Collections.Generic;
using Ceres.ExecutionEngine;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using Ceres.Runtime.Modules;

namespace Ceres.Runtime
{
    public sealed class EvilRuntime
    {
        private readonly CeresVM _vm;

        private Table Global => _vm.Global;

        public EvilRuntime(CeresVM vm)
        {
            _vm = vm;
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

        public DynamicValue Register(string fullyQualifiedName, DynamicValue value, bool replaceIfExists = true)
            => Global.SetUsingPath<PropertyTable>(fullyQualifiedName, value, replaceIfExists);
    }
}
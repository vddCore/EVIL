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

        public void RegisterBuiltInModules()
        {
            RegisterModule<ArrayModule>();
            RegisterModule<CoreModule>();
            RegisterModule<EvilModule>();
            RegisterModule<FsModule>();
            RegisterModule<IoModule>();
            RegisterModule<MathModule>();
            RegisterModule<StringModule>();
            RegisterModule<TableModule>();
            RegisterModule<TimeModule>();
        }

        public DynamicValue RegisterModule<T>() where T : RuntimeModule, new()
            => new T().AttachTo(Global);

        public DynamicValue Register(string fullyQualifiedName, DynamicValue value, bool replaceIfExists = true)
            => Global.SetUsingPath<PropertyTable>(fullyQualifiedName, value, replaceIfExists);
    }
}
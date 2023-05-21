using Ceres.ExecutionEngine;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
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
            RegisterModule<CoreModule>();
            RegisterModule<TimeModule>();
        }

        public DynamicValue RegisterModule<T>() where T : RuntimeModule, new()
            => new T().AttachTo(Global);

        public DynamicValue Register(string fullyQualifiedName, DynamicValue value, bool replaceIfExists = true)
            => Global.SetUsingPath(fullyQualifiedName, value, replaceIfExists);

        public DynamicValue Register(string fullyQualifiedName, double value, bool replaceIfExists = true)
            => Register(fullyQualifiedName, new DynamicValue(value), replaceIfExists);

        public DynamicValue Register(string fullyQualifiedName, string value, bool replaceIfExists = true)
            => Register(fullyQualifiedName, new DynamicValue(value), replaceIfExists);

        public DynamicValue Register(string fullyQualifiedName, bool value, bool replaceIfExists = true)
            => Register(fullyQualifiedName, new DynamicValue(value), replaceIfExists);

        public DynamicValue Register(string fullyQualifiedName, Table value, bool replaceIfExists = true)
            => Register(fullyQualifiedName, new DynamicValue(value), replaceIfExists);

        public DynamicValue Register(string fullyQualifiedName, Chunk value, bool replaceIfExists = true)
            => Register(fullyQualifiedName, new DynamicValue(value), replaceIfExists);

        public DynamicValue Register(string fullyQualifiedName, Fiber value, bool replaceIfExists = true)
            => Register(fullyQualifiedName, new DynamicValue(value), replaceIfExists);

        public DynamicValue Register(string fullyQualifiedName, NativeFunction value, bool replaceIfExists = true)
            => Register(fullyQualifiedName, new DynamicValue(value), replaceIfExists);
    }
}
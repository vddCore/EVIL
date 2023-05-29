using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;

namespace Ceres.Runtime.Modules
{
    public sealed class StringModule : RuntimeModule
    {
        public override string FullyQualifiedName => "str";

        public StringModule()
        {
            AddGetter("empty", (_) => string.Empty);
        }

        [RuntimeModuleFunction("spl")]
        private static DynamicValue Split(Fiber _, params DynamicValue[] args)
        {
            args.ExpectStringAt(0, out var src)
                .ExpectStringAt(1, out var delim);

            var segments = src.Split(delim);
            var table = new Table();

            for (var i = 0; i < segments.Length; i++)
            {
                table.Add(i, segments[i]);
            }

            return table;
        }

        [RuntimeModuleFunction("join")]
        private static DynamicValue Join(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1)
                .ExpectStringAt(0, out string delim);

            return string.Join(delim, args.Skip(1).Select(x => x.ConvertToString().String!));
        }
    }
}
using System.Text;
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

        [RuntimeModuleFunction("rep")]
        private static DynamicValue Repeat(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var str)
                .ExpectIntegerAt(1, out var count);

            var sb = new StringBuilder();
            for (var i = 0; i < (int)count; i++) 
                sb.Append(str);

            return sb.ToString();
        }

        [RuntimeModuleFunction("index_of")]
        private static DynamicValue IndexOf(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectStringAt(0, out var haystack)
                .ExpectStringAt(1, out var needle);
            
            return haystack.IndexOf(needle, StringComparison.InvariantCulture);
        }

        [RuntimeModuleFunction("is_empty")]
        private static DynamicValue IsEmpty(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectStringAt(0, out var value);

            return string.IsNullOrEmpty(value);
        }
        
        [RuntimeModuleFunction("is_whitespace")]
        private static DynamicValue IsWhiteSpace(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectStringAt(0, out var value);

            return string.IsNullOrWhiteSpace(value);
        }

        [RuntimeModuleFunction("lpad")]
        private static DynamicValue LeftPad(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(3)
                .ExpectStringAt(0, out var source)
                .ExpectCharAt(1, out var pad)
                .ExpectIntegerAt(2, out var totalWidth);

            return source.PadLeft((int)totalWidth, pad);
        }
        
        [RuntimeModuleFunction("rpad")]
        private static DynamicValue RightPad(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(3)
                .ExpectStringAt(0, out var source)
                .ExpectCharAt(1, out var pad)
                .ExpectIntegerAt(2, out var totalWidth);

            return source.PadRight((int)totalWidth, pad);
        }
    }
}
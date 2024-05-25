using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime;
using EVIL.Ceres.Runtime.Extensions;
using EVIL.Ceres.TranslationEngine;
using static EVIL.Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace EVIL.Ceres.LanguageTests
{
    public class AssertModule : RuntimeModule
    {
        public override string FullyQualifiedName => "assert";

        public AssertModule()
        {
            var compiler = new Compiler();
            var rootChunk = compiler.Compile("fn __invk(t, expr) -> t.is_true(expr);");

            this["throws"] = compiler.Compile(
                "fn throws(func) {\n" +
                "  if (func !is Function) throw error('This function can only test Functions.');\n" +
                "\n" +
                "  rw val threw = nil;" +
                "\n" +
                "  try {\n" +
                "    func();\n" +
                "    threw = false;\n" +
                "  } catch { threw = true; }\n" +
                "\n" +
                "  throw error { " +
                "    __should_have_thrown: true," +
                "    __threw: threw" +
                "  };\n" +
                "}"
            )["throws"]!;
            
            MetaTable ??= new Table
            {
                { InvokeMetaKey, rootChunk.SubChunks[0] }
            };
        }

        [RuntimeModuleFunction("is_true")]
        private static DynamicValue IsTrue(Fiber _, params DynamicValue[] args)
        {
            args.ExpectBooleanAt(0, out var boolean)
                .OptionalStringAt(1, $"expected expression to be true, but it was false", out var msg);

            if (!boolean)
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }
        
        [RuntimeModuleFunction("is_false")]
        private static DynamicValue IsFalse(Fiber _, params DynamicValue[] args)
        {
            args.ExpectBooleanAt(0, out var boolean)
                .OptionalStringAt(1, $"expected expression to be false, but it was true", out var msg);

            if (boolean)
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }

        [RuntimeModuleFunction("is_of_type")]
        private static DynamicValue IsOfType(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAnyAt(0, out var value)
                .ExpectTypeCodeAt(1, out var typeCode)
                .OptionalStringAt(2, $"expected the type of `{value}' to be `{typeCode}', but was `{value.Type}'", out var msg);

            if (value.Type != typeCode)
            {
                throw new TestAssertionException(msg);
            }

            return Nil;
        }
        
        [RuntimeModuleFunction("equal")]
        private static DynamicValue Equal(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAnyAt(0, out var actual)
                .ExpectAnyAt(1, out var expected)
                .OptionalStringAt(2, $"expected expression to be equal to `{expected}', but was `{actual}' instead", out var msg);

            if (expected != actual)
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }
        
        [RuntimeModuleFunction("greater_than_or_equal")]
        private static DynamicValue GreaterThanOrEqual(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAnyAt(0, out var actual)
                .ExpectAnyAt(1, out var expected)
                .OptionalStringAt(2, $"expected expression to be greater than or equal to `{expected}', but was `{actual}' instead", out var msg);

            if (actual.IsLessThan(expected).Boolean)
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }
        
        [RuntimeModuleFunction("less_than_or_equal")]
        private static DynamicValue LessThanOrEqual(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAnyAt(0, out var actual)
                .ExpectAnyAt(1, out var expected)
                .OptionalStringAt(2, $"expected expression to be less than or equal to `{expected}', but was `{actual}' instead", out var msg);

            if (actual.IsGreaterThan(expected).Boolean)
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }
        
        [RuntimeModuleFunction("approx_equal")]
        private static DynamicValue ApproximatelyEqual(Fiber _, params DynamicValue[] args)
        {
            args.ExpectNumberAt(0, out var actual)
                .ExpectNumberAt(1, out var expected)
                .OptionalIntegerAt(2, 1, out var precision)
                .OptionalStringAt(3, $"expected expression to approximately (rounded to {precision} decimal places) equal `{expected}', but was `{actual}' instead", out var msg);

            if (double.Round(expected, (int)precision) != double.Round(actual, (int)precision))
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }
        
        [RuntimeModuleFunction("not_equal")]
        private static DynamicValue NotEqual(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAnyAt(0, out var actual)
                .ExpectAnyAt(1, out var unexpected)
                .OptionalStringAt(2, $"expected expression to NOT be equal to `{unexpected}', but was `{actual}' instead", out var msg);

            if (unexpected == actual)
            {
                throw new TestAssertionException(msg);
            }
            
            return Nil;
        }
    }
}
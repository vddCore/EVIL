namespace EVIL.Assessment.Tests.VM.Concurrency;

using EVIL.Assessment.Tests.Base;
using EVIL.Core.Common.TypeSystem;

[TestFixture]
public class InvocationAndYieldTests : EvilTest
{
    [Test]
    public void MethodInvocation_CallsWithImplicitSelf()
    {
        var script = """
            fn test() {
                val counter = {
                    count: 10,
                    add: self::fn(n) {
                        self.count += n;
                        ret self.count;
                    }
                };
                ret counter:add(5);
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(15.0));
    }

    [Test]
    public void MethodInvocation_ChainedCalls_ExecuteCorrectly()
    {
        var script = """
            fn test() {
                val builder = {
                    value: 1,
                    double_it: self::fn() {
                        self.value *= 2;
                        ret self;
                    },
                    add: self::fn(n) {
                        self.value += n;
                        ret self;
                    },
                    get: self::fn() -> self.value
                };
                ret builder:double_it():add(10):get();
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(12.0));
    }

    [Test]
    public void MethodInvocation_InTernaryOperator_ExecutesCorrectly()
    {
        var script = """
            fn test() {
                val obj = {
                    greet: self::fn() -> "hello from obj"
                };
                val r1 = true ? obj:greet() : "fallback";
                val r2 = false ? obj:greet() : "fallback";
                ret [r1, r2];
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
        Assert.That(result.Array![0].String, Is.EqualTo("hello from obj"));
        Assert.That(result.Array![1].String, Is.EqualTo("fallback"));
    }

    [Test]
    public void Yield_SimpleFunctionCall()
    {
        var script = """
            fn worker(x) {
                ret x * 2;
            }

            fn test() {
                val res = yield worker(21);
                ret res;
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(42.0));
    }

    [Test]
    public void Yield_MethodInvocation()
    {
        var script = """
            fn test() {
                val worker = {
                    multiplier: 3,
                    compute: self::fn(a, b) {
                        ret (a + b) * self.multiplier;
                    }
                };
                val res = yield worker:compute(4, 6);
                ret res;
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(30.0));
    }

    [Test]
    public void Yield_TableMemberInvocation()
    {
        var script = """
            fn test() {
                val service = {
                    action: fn(x) -> x + 100
                };
                val res = yield service.action(23);
                ret res;
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(123.0));
    }

    [Test]
    public void Yield_ArrayIndexerInvocation()
    {
        var script = """
            fn test() {
                val funcs = [
                    fn(x) -> x + 1,
                    fn(x) -> x * 10
                ];
                val res0 = yield funcs[0](5);
                val res1 = yield funcs[1](5);
                ret [res0, res1];
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
        Assert.That(result.Array![0].Number, Is.EqualTo(6.0));
        Assert.That(result.Array![1].Number, Is.EqualTo(50.0));
    }

    [Test]
    public void Yield_ChainedReturnedFunctionInvocation()
    {
        var script = """
            fn get_worker() {
                ret fn(a, b) -> a + b;
            }

            fn test() {
                val res = yield get_worker()(20, 22);
                ret res;
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(42.0));
    }

    [Test]
    public void Yield_ParenthesizedConditionalInvocation()
    {
        var script = """
            fn add1(x) -> x + 1;
            fn add2(x) -> x + 2;

            fn test() {
                val r1 = yield (true ? add1 : add2)(10);
                val r2 = yield (false ? add1 : add2)(10);
                ret [r1, r2];
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
        Assert.That(result.Array![0].Number, Is.EqualTo(11.0));
        Assert.That(result.Array![1].Number, Is.EqualTo(12.0));
    }
}

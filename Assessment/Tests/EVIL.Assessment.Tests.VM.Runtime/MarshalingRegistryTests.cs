namespace EVIL.Assessment.Tests.VM.Runtime;

using System;
using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine;
using EVIL.VM.ExecutionEngine.Marshaling;
using EVIL.VM.ExecutionEngine.Marshaling.Adapters;
using EVIL.VM.ExecutionEngine.TypeSystem;
using NUnit.Framework;

public class MarshalingRegistryTests
{
    [Test]
    public void BuiltInScalarAdaptersConvertBothDirections()
    {
        var registry = new MarshalingRegistry();

        var number = registry.ToDynamicValue(42);
        var text = registry.ToDynamicValue("hello");
        var flag = registry.ToDynamicValue(true);

        Assert.That(number.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(registry.ToNative<int>(number), Is.EqualTo(42));
        Assert.That(registry.ToNative<string>(text), Is.EqualTo("hello"));
        Assert.That(registry.ToNative<bool>(flag), Is.True);
    }

    [Test]
    public void AdapterResolutionPrefersExactThenMostSpecificType()
    {
        var registry = new MarshalingRegistry();
        registry.Register(new BaseAdapter());
        registry.Register(new MiddleAdapter());

        var value = registry.ToDynamicValue(new LeafValue());

        Assert.That(value.String, Is.EqualTo("middle"));
    }

    [Test]
    public void AdapterResolutionRejectsAmbiguousAssignableTypes()
    {
        var registry = new MarshalingRegistry();
        registry.Register(new LeftAdapter());
        registry.Register(new RightAdapter());

        var exception = Assert.Throws<MarshalingException>(() => registry.ToDynamicValue(new BothValue()));

        Assert.That(exception!.Direction, Is.EqualTo(MarshalingDirection.ClrToEvil));
        Assert.That(exception.Message, Does.Contain("ambiguous"));
    }

    [Test]
    public void DuplicateRegistrationRequiresExplicitReplacement()
    {
        var registry = new MarshalingRegistry();

        Assert.Throws<InvalidOperationException>(() => registry.Register(new StringAdapter()));

        registry.Register(new StringAdapter(), replaceExisting: true);

        Assert.That(registry.ToDynamicValue("value").String, Is.EqualTo("adapted"));
    }

    [Test]
    public void FreezePreventsRegistrationAndConfigurationChanges()
    {
        var registry = new MarshalingRegistry();
        registry.Freeze();

        Assert.That(registry.IsFrozen, Is.True);
        Assert.Throws<InvalidOperationException>(() => registry.Register(new SampleAdapter()));
        Assert.Throws<InvalidOperationException>(() => registry.FallbackMode = MarshalingFallbackMode.Throw);
        Assert.That(registry.ToDynamicValue(42).Number, Is.EqualTo(42));
    }

    [Test]
    public void NullMapsToNilAndNilOnlyMapsToNullableTargets()
    {
        var registry = new MarshalingRegistry();

        var nil = registry.ToDynamicValue(null);

        Assert.That(nil.Type, Is.EqualTo(DynamicValueType.Nil));
        Assert.That(registry.ToNative<string?>(nil), Is.Null);
        Assert.That(registry.ToNative<int?>(nil), Is.Null);
        Assert.Throws<MarshalingException>(() => registry.ToNative<int>(nil));

        var wrappedNull = new DynamicValue((object?)null);
        Assert.That(registry.ToDynamicValue(wrappedNull).Type, Is.EqualTo(DynamicValueType.NativeObject));
        Assert.That(registry.ToNative<string?>(wrappedNull), Is.Null);
    }

    [Test]
    public void IntegerAndDecimalAdaptersRejectPrecisionLoss()
    {
        var registry = new MarshalingRegistry();
        const long exactlyRepresentable = 9_007_199_254_740_992;
        const long notExactlyRepresentable = 9_007_199_254_740_993;

        var exactNumber = registry.ToDynamicValue(exactlyRepresentable);

        Assert.That(registry.ToNative<long>(exactNumber), Is.EqualTo(exactlyRepresentable));
        Assert.Throws<MarshalingException>(() => registry.ToDynamicValue(notExactlyRepresentable));
        Assert.That(registry.ToNative<decimal>(registry.ToDynamicValue(0.5m)), Is.EqualTo(0.5m));
        Assert.Throws<MarshalingException>(() => registry.ToDynamicValue(0.1m));
        Assert.Throws<MarshalingException>(() => registry.ToNative<decimal>(new DynamicValue(0.1)));
        Assert.Throws<MarshalingException>(() => registry.ToNative<int>(new DynamicValue(2.5)));
        Assert.Throws<MarshalingException>(() => registry.ToNative<int>(new DynamicValue(double.PositiveInfinity)));
    }

    [Test]
    public void UnadaptedValuesUseConfiguredNativeObjectFallback()
    {
        var nativeObject = new object();
        var wrappingRegistry = new MarshalingRegistry();

        var wrapped = wrappingRegistry.ToDynamicValue(nativeObject);

        Assert.That(wrapped.Type, Is.EqualTo(DynamicValueType.NativeObject));
        Assert.That(wrappingRegistry.ToNative<object>(wrapped), Is.SameAs(nativeObject));

        var strictRegistry = new MarshalingRegistry(MarshalingFallbackMode.Throw);
        var exception = Assert.Throws<MarshalingException>(() => strictRegistry.ToDynamicValue(nativeObject));
        Assert.That(exception!.Direction, Is.EqualTo(MarshalingDirection.ClrToEvil));
    }

    [Test]
    public void AdapterFailuresKeepTheOriginalException()
    {
        var registry = new MarshalingRegistry();
        registry.Register(new FailingAdapter());

        var exception = Assert.Throws<MarshalingException>(() => registry.ToDynamicValue(new Sample()));

        Assert.That(exception!.SourceType, Is.EqualTo(typeof(Sample).FullName));
        Assert.That(exception.InnerException, Is.TypeOf<ArgumentException>());
    }

    [Test]
    public void OneWayAdaptersReportUnsupportedConversionDirection()
    {
        var registry = new MarshalingRegistry();
        registry.Register(new OneWayAdapter());

        var value = registry.ToDynamicValue(new Sample());
        var exception = Assert.Throws<MarshalingException>(() => registry.ToNative<Sample>(value));

        Assert.That(value.String, Is.EqualTo("one-way"));
        Assert.That(exception!.InnerException, Is.TypeOf<NotSupportedException>());
    }

    [Test]
    public void ContextualAdapterPreservesCyclesWhenItRegistersPlaceholders()
    {
        var registry = new MarshalingRegistry();
        registry.Register(new GraphNodeAdapter());

        var node = new GraphNode("root");
        node.Next = node;

        var evilValue = registry.ToDynamicValue(node);
        var restored = registry.ToNative<GraphNode>(evilValue);

        Assert.That(evilValue.Type, Is.EqualTo(DynamicValueType.Table));
        Assert.That(evilValue.Table!["next"].Table, Is.SameAs(evilValue.Table));
        Assert.That(restored.Next, Is.SameAs(restored));
    }

    [Test]
    public void ContextualAdapterRejectsCyclesWithoutRegisteredPlaceholders()
    {
        var registry = new MarshalingRegistry();
        registry.Register(new UntrackedCycleAdapter());

        var exception = Assert.Throws<MarshalingException>(() => registry.ToDynamicValue(new GraphNode("root")));

        Assert.That(exception!.InnerException, Is.TypeOf<MarshalingException>());
        Assert.That(exception.InnerException!.Message, Does.Contain("cycle"));
    }

    [Test]
    public void VirtualMachinesOwnIndependentRegistriesAndAcceptHostConfiguration()
    {
        var registry = new MarshalingRegistry();
        var first = new VirtualMachine(marshal: registry);
        var second = new VirtualMachine();

        Assert.That(first.Marshal, Is.SameAs(registry));
        Assert.That(second.Marshal, Is.Not.SameAs(first.Marshal));
    }

    private class BaseValue
    {
    }

    private class MiddleValue : BaseValue
    {
    }

    private sealed class LeafValue : MiddleValue
    {
    }

    private sealed class BothValue : ILeftValue, IRightValue
    {
    }

    private interface ILeftValue
    {
    }

    private interface IRightValue
    {
    }

    private sealed class BaseAdapter : IValueAdapter<BaseValue>
    {
        public DynamicValue ToDynamicValue(BaseValue value) => "base";
        public BaseValue FromDynamicValue(DynamicValue value) => new();
    }

    private sealed class MiddleAdapter : IValueAdapter<MiddleValue>
    {
        public DynamicValue ToDynamicValue(MiddleValue value) => "middle";
        public MiddleValue FromDynamicValue(DynamicValue value) => new();
    }

    private sealed class LeftAdapter : IValueAdapter<ILeftValue>
    {
        public DynamicValue ToDynamicValue(ILeftValue value) => "left";
        public ILeftValue FromDynamicValue(DynamicValue value) => new BothValue();
    }

    private sealed class RightAdapter : IValueAdapter<IRightValue>
    {
        public DynamicValue ToDynamicValue(IRightValue value) => "right";
        public IRightValue FromDynamicValue(DynamicValue value) => new BothValue();
    }

    private sealed class Sample
    {
    }

    private sealed class SampleAdapter : IValueAdapter<Sample>
    {
        public DynamicValue ToDynamicValue(Sample value) => "sample";
        public Sample FromDynamicValue(DynamicValue value) => new();
    }

    private sealed class StringAdapter : IValueAdapter<string>
    {
        public DynamicValue ToDynamicValue(string value) => "adapted";
        public string FromDynamicValue(DynamicValue value) => "adapted";
    }

    private sealed class FailingAdapter : IValueAdapter<Sample>
    {
        public DynamicValue ToDynamicValue(Sample value) => throw new ArgumentException("adapter failure");
        public Sample FromDynamicValue(DynamicValue value) => throw new ArgumentException("adapter failure");
    }

    private sealed class OneWayAdapter : IValueAdapter<Sample>
    {
        public DynamicValue ToDynamicValue(Sample value) => "one-way";
    }

    private sealed class GraphNode(string name)
    {
        public string Name { get; } = name;
        public GraphNode? Next { get; set; }
    }

    private sealed class GraphNodeAdapter : IContextualValueAdapter<GraphNode>
    {
        public DynamicValue ToDynamicValue(GraphNode value, MarshalingContext context)
        {
            var table = new EVIL.VM.ExecutionEngine.Collections.Table();
            context.RegisterDynamicValue(value, table);
            table["name"] = value.Name;
            table["next"] = value.Next == null ? DynamicValue.Nil : context.ToDynamicValue(value.Next);
            return table;
        }

        public GraphNode FromDynamicValue(DynamicValue value, MarshalingContext context)
        {
            if (value.Type != DynamicValueType.Table)
            {
                throw new InvalidCastException("Expected an EVIL table.");
            }

            var table = value.Table!;
            var node = new GraphNode(table["name"].String!);
            context.RegisterNativeObject(value, node);

            if (table["next"].Type != DynamicValueType.Nil)
            {
                node.Next = context.ToNative<GraphNode>(table["next"]);
            }

            return node;
        }
    }

    private sealed class UntrackedCycleAdapter : IContextualValueAdapter<GraphNode>
    {
        public DynamicValue ToDynamicValue(GraphNode value, MarshalingContext context)
            => context.ToDynamicValue(value);

        public GraphNode FromDynamicValue(DynamicValue value, MarshalingContext context)
            => context.ToNative<GraphNode>(value);
    }
}
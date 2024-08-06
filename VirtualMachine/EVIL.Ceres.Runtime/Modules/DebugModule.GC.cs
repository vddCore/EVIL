namespace EVIL.Ceres.Runtime.Modules;

using System;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;

public partial class DebugModule
{
    [RuntimeModuleFunction("gc.mem.get_used_total")]
    [EvilDocFunction(
        "Attempts to retrieve the amount of managed memory currently thought to be allocated.",
        Returns = "The amount of managed memory - in bytes - currently thought to be allocated.",
        ReturnType = DynamicValueType.Number
    )]
    [EvilDocArgument(
        "force_full_collection",
        "`true` to indicate waiting for GC, `false` to return an immediate value.",
        DynamicValueType.Boolean,
        DefaultValue = "false"
    )]
    private static DynamicValue GcMemGetTotal(Fiber _, params DynamicValue[] args)
    {
        args.ExpectAtMost(1);
        args.OptionalBooleanAt(0, false, out var forceFullCollection);

        return GC.GetTotalMemory(forceFullCollection);
    }

    [RuntimeModuleFunction("gc.mem.get_info")]
    [EvilDocFunction(
        "Retrieves memory statistics of the garbage collector.",
        Returns = "A table containing information about GC memory statistics.",
        ReturnType = DynamicValueType.Table
    )]
    private static DynamicValue GcMemGetInfo(Fiber _, params DynamicValue[] args)
    {
        args.ExpectNone();

        var info = GC.GetGCMemoryInfo();

        return new Table
        {
            ["index"] = info.Index,
            ["generation"] = info.Generation,
            ["is_compacted"] = info.Compacted,
            ["is_concurrent"] = info.Concurrent,
            ["finalization_pending_count"] = info.FinalizationPendingCount,
            ["fragmented_bytes"] = info.FragmentedBytes,
            ["promoted_bytes"] = info.PromotedBytes,
            ["heap_size_bytes"] = info.HeapSizeBytes,
            ["total_committed_bytes"] = info.TotalCommittedBytes,
            ["total_avail_mem_bytes"] = info.TotalAvailableMemoryBytes,
            ["pause_time_percent"] = info.PauseTimePercentage,
            ["pinned_object_count"] = info.PinnedObjectsCount
        };
    }

    [RuntimeModuleFunction("gc.collect")]
    [EvilDocFunction("Forces a compacting asynchronous aggressive garbage collection for all generations.")]
    private static DynamicValue GcCollect(Fiber _, params DynamicValue[] args)
    {
        args.ExpectNone();
        GC.Collect(
            GC.MaxGeneration,
            GCCollectionMode.Aggressive,
            false,
            true
        );

        return DynamicValue.Nil;
    }
}
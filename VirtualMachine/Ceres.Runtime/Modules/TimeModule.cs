using System;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.Runtime.Modules
{
    public sealed class TimeModule : RuntimeModule
    {
        public override string FullyQualifiedName => "time";

        [RuntimeModuleGetter("now")]
        [EvilDocProperty(
            EvilDocPropertyMode.Get,
            "Retrieves the current date and time as a Table containing specific details.  \n\n" +
            "The returned Table will use the following layout:  \n" +
            "```\n" +
            "{\n" +
            "  day: Number\n" +
            "  month: Number\n" +
            "  year: Number\n" +
            "  hour: Number\n" +
            "  minute: Number\n" +
            "  second: Number\n" +
            "  millisecond: Number\n" +
            "  microsecond: Number\n" +
            "  nanosecond: Number\n" +
            "  ticks: Number\n" +
            "  day_of_year: Number\n" +
            "  day_of_week: Number\n" +
            "  day_of_week_name: String\n" +
            "  as_stamp_ms: Function\n" +
            "  as_stamp_secs: Function\n" +
            "}\n" +
            "```\n" +
            "`as_stamp_ms` and `as_stamp_secs` functions work analogously to `stamp.ms` and `stamp.secs`.",
            ReturnType = DynamicValueType.Table
        )]
        private static DynamicValue GetNow(DynamicValue key)
            => CreateDateTimeTable(DateTime.Now);

        [RuntimeModuleGetter("stamp.ms")]
        [EvilDocProperty(
            EvilDocPropertyMode.Get,
            "Retrieves the current time as a Unix timestamp in milliseconds.",
            ReturnType = DynamicValueType.Number
        )]
        private static DynamicValue GetStampMillis(DynamicValue key) 
            => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        [RuntimeModuleGetter("stamp.secs")]
        [EvilDocProperty(
            EvilDocPropertyMode.Get,
            "Retrieves the current time as a Unix timestamp in seconds.",
            ReturnType = DynamicValueType.Number
        )]
        private static DynamicValue GetStampSeconds(DynamicValue key) 
            => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        private static Table CreateDateTimeTable(DateTime dt)
        {
            var ret = new Table
            {
                { "day", dt.Day },
                { "month", dt.Month },
                { "year", dt.Year },
                { "hour", dt.Hour },
                { "minute", dt.Minute },
                { "second", dt.Second },
                { "millisecond", dt.Millisecond },
                { "microsecond", dt.Microsecond },
                { "nanosecond", dt.Nanosecond },
                { "ticks", dt.Ticks },
                { "day_of_year", dt.DayOfYear },
                { "day_of_week", (int)dt.DayOfWeek },
                { "day_of_week_name", dt.DayOfWeek.ToString() },
                {
                    "as_stamp_ms", 
                    new((_, args) =>
                    {
                        args.ExpectNone();
                        return new DateTimeOffset(dt).ToUnixTimeMilliseconds();
                    })
                },
                {
                    "as_stamp_secs", 
                    new((_, args) =>
                    {
                        args.ExpectNone();
                        return new DateTimeOffset(dt).ToUnixTimeSeconds();
                    })
                }
            };

            return ret;
        }
    }
}
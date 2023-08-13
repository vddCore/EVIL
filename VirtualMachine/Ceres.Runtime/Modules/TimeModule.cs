using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.Runtime.Modules
{
    public sealed class TimeModule : RuntimeModule
    {
        public override string FullyQualifiedName => "time";

        [RuntimeModuleGetter("now", ReturnType = DynamicValueType.Table)]
        private static DynamicValue GetNow(DynamicValue key)
            => CreateDateTimeTable(DateTime.Now);

        [RuntimeModuleGetter("stamp.ms", ReturnType = DynamicValueType.Number)]
        private static DynamicValue GetStampMillis(DynamicValue key) 
            => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        [RuntimeModuleGetter("stamp.secs", ReturnType = DynamicValueType.Number)]
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
                { "day_of_week", dt.DayOfWeek.ToString() },
                {
                    "as_stamp", 
                    new((context, args) =>
                    {
                        args.ExpectNone();
                        return new DateTimeOffset(dt).ToUnixTimeMilliseconds();
                    })
                }
            };

            return ret;
        }
    }
}
using System;
using Ceres.Runtime.Modules;
using NUnit.Framework;
using Shouldly;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public class TimeModuleTest : ModuleTest<TimeModule>
    {
        [Test]
        public void TimeNow()
        {
            var now = DateTime.Now;
            var stamp = new DateTimeOffset(now).ToUnixTimeMilliseconds();

            var t = EvilTestResult(
                "fn test() {" +
                "   val t = time.now;" +
                "   ret { now: t, stamp: t.as_stamp() }; " +
                "}"
            ).Table!;

            var evilNow = t["now"].Table!;
            evilNow["day"].ShouldBe(now.Day);
            evilNow["month"].ShouldBe(now.Month);
            evilNow["year"].ShouldBe(now.Year);
            evilNow["hour"].ShouldBe(now.Hour);
            evilNow["minute"].ShouldBe(now.Minute);
            ((double)evilNow["second"]).ShouldBeInRange(now.Second, now.Second + 2);
            ((double)evilNow["millisecond"]).ShouldBeInRange(0, 999);
            ((double)evilNow["microsecond"]).ShouldBeInRange(0, 999);
            ((double)evilNow["nanosecond"]).ShouldBeInRange(0, 1000);
            ((double)evilNow["ticks"]).ShouldBeInRange(now.Ticks, now.Ticks + 20000000);
            evilNow["day_of_year"].ShouldBe(now.DayOfYear);
            evilNow["day_of_week"].ShouldBe(now.DayOfWeek.ToString());

            var evilStamp = t["stamp"].Number!;
            evilStamp.ShouldBeInRange(stamp, stamp + 100);
        }

        [Test]
        public void StampMs()
        {
            var ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var evilMs = EvilTestResult(
                "fn test() -> time.stamp.ms;"
            ).Number;

            evilMs.ShouldBeInRange(ms, ms + 1000);
        }
        
        [Test]
        public void StampSecs()
        {
            var secs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var evilSecs = EvilTestResult(
                "fn test() -> time.stamp.secs;"
            ).Number;

            evilSecs.ShouldBeInRange(secs, secs + 2);
        }
    }
}
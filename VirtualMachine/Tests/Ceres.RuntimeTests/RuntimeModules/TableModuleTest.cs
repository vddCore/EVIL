using Ceres.Runtime.Modules;
using NUnit.Framework;
using Shouldly;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public class TableModuleTest : ModuleTest<TableModule>
    {
        [Test]
        public void Clear()
        {
            var t = EvilTestResult(
                "fn test() {" +
                "   val t = { 1, 2, 3, 4 };" +
                "   tbl.clear(t);" +
                "   ret t;" +
                "}"
            ).Table!;

            t.Length.ShouldBe(0);
        }

        [Test]
        public void Freeze()
        {
            var t = EvilTestResult(
                "fn test() {" +
                "   val t = {};" +
                "   ret tbl.freeze(t);" +
                "}"
            ).Table!;

            t.IsFrozen.ShouldBe(true);
        }
        
        [Test]
        public void Unfreeze()
        {
            var t = EvilTestResult(
                "fn test() {" +
                "   val t = {};" +
                "   ret tbl.unfreeze(tbl.freeze(t));" +
                "}"
            ).Table!;

            t.IsFrozen.ShouldBe(false);
        }
        
        [Test]
        public void IsFrozen()
        {
            var b = EvilTestResult(
                "fn test() {" +
                "   val t = {};" +
                "   tbl.freeze(t);" +
                "   ret tbl.is_frozen(t);" +
                "}"
            ).Boolean!;

            b.ShouldBe(true);
        }

        [Test]
        public void Keys()
        {
            var t = EvilTestResult(
                "fn test() {" +
                "   val t = {" +
                "       this: 'is'," +
                "       a: 'test'," +
                "       of: 'evil runtime'" +
                "   };" +
                "   ret tbl.keys(t);" +
                "}"
            ).Table!;

            t.Length.ShouldBe(3);
            foreach (var kvp in t)
            {
                kvp.Value.ShouldBeOneOf("this", "a", "of");
            }
        }
        
        [Test]
        public void Values()
        {
            var t = EvilTestResult(
                "fn test() {" +
                "   val t = {" +
                "       this: 'is'," +
                "       a: 'test'," +
                "       of: 'evil runtime'" +
                "   };" +
                "   ret tbl.values(t);" +
                "}"
            ).Table!;

            t.Length.ShouldBe(3);
            foreach (var kvp in t)
            {
                kvp.Value.ShouldBeOneOf("is", "test", "evil runtime");
            }
        }

        [Test]
        public void ShallowCopy()
        {
            var t = EvilTestResult(
                "fn test() {" +
                "   val t1 = { 1, 2, 3 };" +
                "   val t2 = { 'test', t1 };" +
                "   val t3 = tbl.cpy(t2);" +
                "" +
                "   ret { t1: t1, t2: t2, t3: t3 };" +
                "}"
            ).Table!;

            var t1 = t["t1"];
            var t2 = t["t2"].Table!;
            var t3 = t["t3"].Table!;

            t1.ShouldBeEquivalentTo(t3[1]);
            t1.ShouldBeEquivalentTo(t2[1]);
            t3[1].ShouldBeEquivalentTo(t2[1]);
        }
        
        [Test]
        public void DeepCopy()
        {
            var t = EvilTestResult(
                "fn test() {" +
                "   val t1 = { 1, 2, 3 };" +
                "   val t2 = { 'test', t1 };" +
                "   val t3 = tbl.cpy(t2, true);" +
                "" +
                "   ret { t1: t1, t2: t2, t3: t3 };" +
                "}"
            ).Table!;

            var t1 = t["t1"].Table!;
            var t2 = t["t2"].Table!;
            var t3 = t["t3"].Table!;

            t1.IsDeeplyEqualTo(t3[1].Table!).ShouldBeTrue();
            t1.IsDeeplyEqualTo(t2[1].Table!).ShouldBeTrue();
            t3[1].Table!.IsDeeplyEqualTo(t2[1].Table!).ShouldBeTrue();
            t2[1].Table!.ShouldNotBeSameAs(t3[1].Table!);
        }
    }
}
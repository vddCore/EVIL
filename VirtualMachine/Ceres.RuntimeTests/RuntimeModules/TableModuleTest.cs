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
                "   var t = { 1, 2, 3, 4 };" +
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
                "   var t = {};" +
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
                "   var t = {};" +
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
                "   var t = {};" +
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
                "   var t = {" +
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
                "   var t = {" +
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
    }
}
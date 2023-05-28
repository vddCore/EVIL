using Ceres.Runtime.Modules;
using NUnit.Framework;

namespace Ceres.RuntimeTests.RuntimeModules
{
    public class StringModuleTest : ModuleTest<StringModule>
    {
        [Test]
        public void Split()
        {
            var t = RunEvilCode(
                "fn test() -> str.spl(\"abc|def|ghi\", \"|\");"
            ).Table!;

            Assert.That(t[0].String, Is.EqualTo("abc"));
            Assert.That(t[1].String, Is.EqualTo("def"));
            Assert.That(t[2].String, Is.EqualTo("ghi"));
        }
    }
}
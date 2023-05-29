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

        [Test]
        public void JoinEmpty()
        {
            var s = RunEvilCode("fn test() -> str.join(str.empty);").String!;
            Assert.That(s, Is.EqualTo(string.Empty));
        }
        
        [Test]
        public void JoinVarious()
        {
            var s = RunEvilCode("fn test() -> str.join(\"|\", 1, \"test\", true);").String!;
            Assert.That(s, Is.EqualTo("1|test|true"));
        }

        [Test]
        public void Repeat()
        {
            var s = RunEvilCode("fn test() -> str.rep(\"test\", 4);").String!;
            Assert.That(s, Is.EqualTo("testtesttesttest"));
        }

        [Test]
        public void IndexOf()
        {
            var n = RunEvilCode("fn test() -> str.index_of(\"hello, world!\", \"w\");").Number;
            Assert.That(n, Is.EqualTo(7));
        }
        
        [Test]
        public void IsEmpty()
        {
            var b = RunEvilCode("fn test() -> str.is_empty(\"\");").Boolean;
            Assert.That(b, Is.True);
        }
        
        [Test]
        public void IsWhiteSpace()
        {
            var b = RunEvilCode("fn test() -> str.is_whitespace(\"  \");").Boolean;
            Assert.That(b, Is.True);
        }

        [Test]
        public void LeftPad()
        {
            var s = RunEvilCode("fn test() -> str.lpad(\"2137\", \"0\", 8);").String!;
            Assert.That(s, Is.EqualTo("00002137"));
        }
        
        [Test]
        public void RightPad()
        {
            var s = RunEvilCode("fn test() -> str.rpad(\"2137\", \"0\", 8);").String!;
            Assert.That(s, Is.EqualTo("21370000"));
        }
    }
}
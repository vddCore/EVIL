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
                "fn test() -> str.spl('abc|def|ghi', '|');"
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
            var s = RunEvilCode("fn test() -> str.join('|', 1, 'test', true);").String!;
            Assert.That(s, Is.EqualTo("1|test|true"));
        }

        [Test]
        public void Repeat()
        {
            var s = RunEvilCode("fn test() -> str.rep('test', 4);").String!;
            Assert.That(s, Is.EqualTo("testtesttesttest"));
        }

        [Test]
        public void IndexOf()
        {
            var n = RunEvilCode("fn test() -> str.index_of('hello, world!', 'w');").Number;
            Assert.That(n, Is.EqualTo(7));
        }
        
        [Test]
        public void IsEmpty()
        {
            var b = RunEvilCode("fn test() -> str.is_empty('');").Boolean;
            Assert.That(b, Is.True);
        }
        
        [Test]
        public void IsWhiteSpace()
        {
            var b = RunEvilCode("fn test() -> str.is_whitespace('');").Boolean;
            Assert.That(b, Is.True);
        }

        [Test]
        public void LeftPad()
        {
            var s = RunEvilCode("fn test() -> str.lpad('2137', '0', 8);").String!;
            Assert.That(s, Is.EqualTo("00002137"));
        }
        
        [Test]
        public void RightPad()
        {
            var s = RunEvilCode("fn test() -> str.rpad('2137', '0', 8);").String!;
            Assert.That(s, Is.EqualTo("21370000"));
        }
        
        [Test]
        public void UpperCase()
        {
            var s = RunEvilCode("fn test() -> str.ucase('i was lowercase. ą.');").String!;
            Assert.That(s, Is.EqualTo("I WAS LOWERCASE. Ą."));
        }
        
        [Test]
        public void LowerCase()
        {
            var s = RunEvilCode("fn test() -> str.lcase('I WAS UPPERCASE ONCE. Ą.');").String!;
            Assert.That(s, Is.EqualTo("i was uppercase once. ą."));
        }

        [Test]
        public void SubstringFrom()
        {
            var s = RunEvilCode("fn test() -> str.sub('hello, world', 7);").String!;
            Assert.That(s, Is.EqualTo("world"));
        }
        
        [Test]
        public void SubstringFromWithNil()
        {
            var s = RunEvilCode("fn test() -> str.sub('hello, world', 7, nil);").String!;
            Assert.That(s, Is.EqualTo("world"));
        }
        
        [Test]
        public void SubstringFromTo()
        {
            var s = RunEvilCode("fn test() -> str.sub('hello, world', 3, 7);").String!;
            Assert.That(s, Is.EqualTo("lo, w"));
        }

        [Test]
        public void StartsWith()
        {
            var s = RunEvilCode("fn test() -> str.starts_with('hello, world', 'hel');").Boolean!;
            Assert.That(s, Is.EqualTo(true));
        }
        
        [Test]
        public void EndsWith()
        {
            var s = RunEvilCode("fn test() -> str.ends_with('hello, world', 'orld');").Boolean!;
            Assert.That(s, Is.EqualTo(true));
        }

        [Test]
        public void RegexMatch()
        {
            var t = RunEvilCode(
                "fn test() -> str.rmatch(" +
                "   'This 21 is 37 an example 00 of 12 a test string 99.'," +
               @"   '(\\w+) (\\d+)'" +
                ");"
            ).Table!;

            Assert.That(t.Length, Is.EqualTo(5));
            Assert.That(t[0].Table!["value"].String!, Is.EqualTo("This 21"));
            Assert.That(t[1].Table!["value"].String!, Is.EqualTo("is 37"));
            Assert.That(t[2].Table!["value"].String!, Is.EqualTo("example 00"));
            Assert.That(t[3].Table!["value"].String!, Is.EqualTo("of 12"));
            Assert.That(t[4].Table!["value"].String!, Is.EqualTo("string 99"));
        }
    }
}